using SGA.Data;
using SGA.DTOs;
using SGA.Models;
using Microsoft.EntityFrameworkCore;

namespace SGA.Services
{
    public class RolService : IRolService
    {
        private readonly SgaDbContext _db;
        private readonly ILogger<RolService> _logger;

        public RolService(SgaDbContext db, ILogger<RolService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<List<RolResponse>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
        {
            return await _db.Roles
                .AsNoTracking()
                .OrderBy(r => r.NombreRol)
                .Select(r => ToResponse(r))
                .ToListAsync(cancellationToken);
        }

        public async Task<RolResponse> ObtenerPorIdAsync(int rolId, CancellationToken cancellationToken = default)
        {
            var rol = await _db.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RolId == rolId, cancellationToken);

            if (rol is null)
                throw new EntidadNoEncontradaException($"No existe el Rol con Id {rolId}.");

            return ToResponse(rol);
        }

        public async Task<RolResponse> CrearAsync(RolRequest request, CancellationToken cancellationToken = default)
        {
            var existe = await _db.Roles
                .AsNoTracking()
                .AnyAsync(r => r.NombreRol == request.NombreRol.Trim(), cancellationToken);

            if (existe)
                throw new ValidacionException($"Ya existe un rol con el nombre '{request.NombreRol}'.");

            var rol = new Rol
            {
                NombreRol = request.NombreRol.Trim(),
                Activo = request.Activo
            };

            _db.Roles.Add(rol);
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Rol creado con Id {RolId}", rol.RolId);
            return ToResponse(rol);
        }

        public async Task<RolResponse> ActualizarAsync(int rolId, RolRequest request, CancellationToken cancellationToken = default)
        {
            var rol = await _db.Roles
                .FirstOrDefaultAsync(r => r.RolId == rolId, cancellationToken);

            if (rol is null)
                throw new EntidadNoEncontradaException($"No existe el Rol con Id {rolId}.");

            var nombreDuplicado = await _db.Roles
                .AsNoTracking()
                .AnyAsync(r => r.NombreRol == request.NombreRol.Trim() && r.RolId != rolId, cancellationToken);

            if (nombreDuplicado)
                throw new ValidacionException($"Ya existe otro rol con el nombre '{request.NombreRol}'.");

            rol.NombreRol = request.NombreRol.Trim();
            rol.Activo = request.Activo;

            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Rol {RolId} actualizado.", rolId);
            return ToResponse(rol);
        }

        private static RolResponse ToResponse(Rol r) => new()
        {
            RolId = r.RolId,
            NombreRol = r.NombreRol,
            Activo = r.Activo
        };
    }
}
