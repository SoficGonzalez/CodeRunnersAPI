using SGA.Data;
using SGA.DTOs;
using SGA.Models;
using Microsoft.EntityFrameworkCore;

namespace SGA.Services
{
    public class SedeService : ISedeService
    {
        private readonly SgaDbContext _db;
        private readonly ILogger<SedeService> _logger;

        public SedeService(SgaDbContext db, ILogger<SedeService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<List<SedeResponse>> ObtenerTodasAsync(CancellationToken cancellationToken = default)
        {
            return await _db.Sedes
                .AsNoTracking()
                .OrderBy(s => s.Nombre)
                .Select(s => ToResponse(s))
                .ToListAsync(cancellationToken);
        }

        public async Task<SedeResponse> ObtenerPorIdAsync(int sedeId, CancellationToken cancellationToken = default)
        {
            var sede = await _db.Sedes
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SedeId == sedeId, cancellationToken);

            if (sede is null)
                throw new EntidadNoEncontradaException($"No existe la Sede con Id {sedeId}.");

            return ToResponse(sede);
        }

        public async Task<SedeResponse> CrearAsync(SedeRequest request, CancellationToken cancellationToken = default)
        {
            var existe = await _db.Sedes
                .AsNoTracking()
                .AnyAsync(s => s.Nombre == request.Nombre.Trim(), cancellationToken);

            if (existe)
                throw new ValidacionException($"Ya existe una sede con el nombre '{request.Nombre}'.");

            var sede = new Sede
            {
                Nombre = request.Nombre.Trim(),
                Direccion = request.Direccion.Trim(),
                Departamento = request.Departamento?.Trim(),
                Activa = request.Activa,
                FechaCreacion = DateTime.UtcNow
            };

            _db.Sedes.Add(sede);
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Sede creada con Id {SedeId}", sede.SedeId);
            return ToResponse(sede);
        }

        public async Task<SedeResponse> ActualizarAsync(int sedeId, SedeRequest request, CancellationToken cancellationToken = default)
        {
            var sede = await _db.Sedes
                .FirstOrDefaultAsync(s => s.SedeId == sedeId, cancellationToken);

            if (sede is null)
                throw new EntidadNoEncontradaException($"No existe la Sede con Id {sedeId}.");

            var nombreDuplicado = await _db.Sedes
                .AsNoTracking()
                .AnyAsync(s => s.Nombre == request.Nombre.Trim() && s.SedeId != sedeId, cancellationToken);

            if (nombreDuplicado)
                throw new ValidacionException($"Ya existe otra sede con el nombre '{request.Nombre}'.");

            sede.Nombre = request.Nombre.Trim();
            sede.Direccion = request.Direccion.Trim();
            sede.Departamento = request.Departamento?.Trim();
            sede.Activa = request.Activa;

            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Sede {SedeId} actualizada.", sedeId);
            return ToResponse(sede);
        }

        private static SedeResponse ToResponse(Sede s) => new()
        {
            SedeId = s.SedeId,
            Nombre = s.Nombre,
            Direccion = s.Direccion,
            Departamento = s.Departamento,
            Activa = s.Activa,
            FechaCreacion = s.FechaCreacion
        };
    }
}
