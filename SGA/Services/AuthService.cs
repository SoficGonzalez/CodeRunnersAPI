using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SGA.Data;
using SGA.DTOs;
using SGA.Identity;
using SGA.Models;

namespace SGA.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly SgaDbContext _db;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<AppIdentityUser> userManager,
            SgaDbContext db,
            IJwtService jwtService,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _db = db;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var identityUser = await _userManager.FindByEmailAsync(request.Correo);

            if (identityUser is null || !await _userManager.CheckPasswordAsync(identityUser, request.Password))
                throw new ValidacionException("Credenciales inválidas.");

            var roles = await _userManager.GetRolesAsync(identityUser);

            // Traer datos del usuario de negocio
            var usuario = await _db.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UsuarioId == identityUser.UsuarioId, cancellationToken);

            return new LoginResponse
            {
                Token = _jwtService.GenerarToken(identityUser, roles),
                NombreCompleto = usuario?.NombreCompleto ?? identityUser.Email!,
                Correo = identityUser.Email!,
                Roles = roles.ToList()
            };
        }

        public async Task<UsuarioResponse> RegistrarAsync(RegistrarUsuarioRequest request, CancellationToken cancellationToken = default)
        {
            // Validar Rol y Sede
            var rol = await _db.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RolId == request.RolId, cancellationToken);

            if (rol is null)
                throw new EntidadNoEncontradaException($"No existe el Rol con Id {request.RolId}.");

            if (request.SedeId.HasValue)
            {
                var sedeExiste = await _db.Sedes
                    .AsNoTracking()
                    .AnyAsync(s => s.SedeId == request.SedeId.Value, cancellationToken);

                if (!sedeExiste)
                    throw new EntidadNoEncontradaException($"No existe la Sede con Id {request.SedeId}.");
            }

            // Verificar duplicados
            if (await _userManager.FindByEmailAsync(request.Correo) is not null)
                throw new ValidacionException($"Ya existe un usuario con el correo '{request.Correo}'.");

            var correoEnBd = await _db.Usuarios
                .AsNoTracking()
                .AnyAsync(u => u.NombreUsuario == request.NombreUsuario.Trim(), cancellationToken);

            if (correoEnBd)
                throw new ValidacionException($"Ya existe un usuario con el nombre '{request.NombreUsuario}'.");

            // Crear usuario de negocio
            var usuario = new Usuario
            {
                NombreUsuario = request.NombreUsuario.Trim(),
                NombreCompleto = request.NombreCompleto.Trim(),
                Correo = request.Correo.Trim(),
                RolId = request.RolId,
                SedeId = request.SedeId,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };

            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync(cancellationToken);

            // Crear Identity user
            var identityUser = new AppIdentityUser
            {
                UserName = request.Correo.Trim(),
                Email = request.Correo.Trim(),
                UsuarioId = usuario.UsuarioId
            };

            var result = await _userManager.CreateAsync(identityUser, request.Password);

            if (!result.Succeeded)
            {
                // Rollback del usuario de negocio
                _db.Usuarios.Remove(usuario);
                await _db.SaveChangesAsync(cancellationToken);
                var errores = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ValidacionException($"Error al crear usuario: {errores}");
            }

            // Asignar rol de Identity (nombre del rol)
            await _userManager.AddToRoleAsync(identityUser, rol.NombreRol);

            _logger.LogInformation("Usuario registrado con Id {UsuarioId}", usuario.UsuarioId);

            return new UsuarioResponse
            {
                UsuarioId = usuario.UsuarioId,
                NombreUsuario = usuario.NombreUsuario,
                NombreCompleto = usuario.NombreCompleto,
                Correo = usuario.Correo,
                RolId = usuario.RolId,
                NombreRol = rol.NombreRol,
                SedeId = usuario.SedeId,
                Activo = usuario.Activo,
                FechaCreacion = usuario.FechaCreacion
            };
        }
    }
}
