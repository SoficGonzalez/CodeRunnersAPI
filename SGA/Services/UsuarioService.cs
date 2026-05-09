using SGA.Data;
using SGA.DTOs;
using SGA.Models;
using Microsoft.EntityFrameworkCore;

namespace SGA.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly SgaDbContext _db;
        private readonly ILogger<UsuarioService> _logger;

        public UsuarioService(SgaDbContext db, ILogger<UsuarioService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<List<UsuarioResponse>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
        {
            return await _db.Usuarios
                .AsNoTracking()
                .Include(u => u.Rol)
                .Include(u => u.Sede)
                .OrderBy(u => u.NombreCompleto)
                .Select(u => ToResponse(u))
                .ToListAsync(cancellationToken);
        }

        public async Task<UsuarioResponse> ObtenerPorIdAsync(int usuarioId, CancellationToken cancellationToken = default)
        {
            var usuario = await _db.Usuarios
                .AsNoTracking()
                .Include(u => u.Rol)
                .Include(u => u.Sede)
                .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId, cancellationToken);

            if (usuario is null)
                throw new EntidadNoEncontradaException($"No existe el Usuario con Id {usuarioId}.");

            return ToResponse(usuario);
        }

        public async Task<UsuarioResponse> CrearAsync(UsuarioRequest request, CancellationToken cancellationToken = default)
        {
            await ValidarRolYSedeAsync(request.RolId, request.SedeId, cancellationToken);

            var usernameDuplicado = await _db.Usuarios
                .AsNoTracking()
                .AnyAsync(u => u.NombreUsuario == request.NombreUsuario.Trim(), cancellationToken);

            if (usernameDuplicado)
                throw new ValidacionException($"Ya existe un usuario con el nombre '{request.NombreUsuario}'.");

            var correoDuplicado = await _db.Usuarios
                .AsNoTracking()
                .AnyAsync(u => u.Correo == request.Correo.Trim(), cancellationToken);

            if (correoDuplicado)
                throw new ValidacionException($"Ya existe un usuario con el correo '{request.Correo}'.");

            var usuario = new Usuario
            {
                NombreUsuario = request.NombreUsuario.Trim(),
                NombreCompleto = request.NombreCompleto.Trim(),
                Correo = request.Correo.Trim(),
                RolId = request.RolId,
                SedeId = request.SedeId,
                Activo = request.Activo,
                FechaCreacion = DateTime.UtcNow
            };

            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Usuario creado con Id {UsuarioId}", usuario.UsuarioId);
            return await ObtenerPorIdAsync(usuario.UsuarioId, cancellationToken);
        }

        public async Task<UsuarioResponse> ActualizarAsync(int usuarioId, UsuarioRequest request, CancellationToken cancellationToken = default)
        {
            var usuario = await _db.Usuarios
                .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId, cancellationToken);

            if (usuario is null)
                throw new EntidadNoEncontradaException($"No existe el Usuario con Id {usuarioId}.");

            await ValidarRolYSedeAsync(request.RolId, request.SedeId, cancellationToken);

            var usernameDuplicado = await _db.Usuarios
                .AsNoTracking()
                .AnyAsync(u => u.NombreUsuario == request.NombreUsuario.Trim() && u.UsuarioId != usuarioId, cancellationToken);

            if (usernameDuplicado)
                throw new ValidacionException($"Ya existe otro usuario con el nombre '{request.NombreUsuario}'.");

            var correoDuplicado = await _db.Usuarios
                .AsNoTracking()
                .AnyAsync(u => u.Correo == request.Correo.Trim() && u.UsuarioId != usuarioId, cancellationToken);

            if (correoDuplicado)
                throw new ValidacionException($"Ya existe otro usuario con el correo '{request.Correo}'.");

            usuario.NombreUsuario = request.NombreUsuario.Trim();
            usuario.NombreCompleto = request.NombreCompleto.Trim();
            usuario.Correo = request.Correo.Trim();
            usuario.RolId = request.RolId;
            usuario.SedeId = request.SedeId;
            usuario.Activo = request.Activo;

            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Usuario {UsuarioId} actualizado.", usuarioId);
            return await ObtenerPorIdAsync(usuarioId, cancellationToken);
        }

        private async Task ValidarRolYSedeAsync(int rolId, int? sedeId, CancellationToken cancellationToken)
        {
            var rolExiste = await _db.Roles
                .AsNoTracking()
                .AnyAsync(r => r.RolId == rolId, cancellationToken);

            if (!rolExiste)
                throw new EntidadNoEncontradaException($"No existe el Rol con Id {rolId}.");

            if (sedeId.HasValue)
            {
                var sedeExiste = await _db.Sedes
                    .AsNoTracking()
                    .AnyAsync(s => s.SedeId == sedeId.Value, cancellationToken);

                if (!sedeExiste)
                    throw new EntidadNoEncontradaException($"No existe la Sede con Id {sedeId}.");
            }
        }

        private static UsuarioResponse ToResponse(Usuario u) => new()
        {
            UsuarioId = u.UsuarioId,
            NombreUsuario = u.NombreUsuario,
            NombreCompleto = u.NombreCompleto,
            Correo = u.Correo,
            RolId = u.RolId,
            NombreRol = u.Rol?.NombreRol ?? string.Empty,
            SedeId = u.SedeId,
            NombreSede = u.Sede?.Nombre,
            Activo = u.Activo,
            FechaCreacion = u.FechaCreacion
        };
    }
}
