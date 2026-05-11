using Microsoft.EntityFrameworkCore;
using SGA.Data;
using SGA.DTOs;
using SGA.Models;

namespace SGA.Services;

public class EstadoPlantillaService : IEstadoPlantillaService
{
    private readonly SgaDbContext _db;
    private readonly ILogger<EstadoPlantillaService> _logger;

    public EstadoPlantillaService(SgaDbContext db, ILogger<EstadoPlantillaService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<EstadoPlantillaResponse>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _db.EstadosPlantilla
            .AsNoTracking()
            .OrderBy(e => e.Orden)
            .ThenBy(e => e.Nombre)
            .ToListAsync(cancellationToken);

        return rows.Select(ToResponse).ToList();
    }

    public async Task<EstadoPlantillaResponse> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var e = await _db.EstadosPlantilla.AsNoTracking().FirstOrDefaultAsync(x => x.EstadoPlantillaId == id, cancellationToken);
        if (e is null)
            throw new EntidadNoEncontradaException($"No existe el EstadoPlantilla con Id {id}.");
        return ToResponse(e);
    }

    public async Task<EstadoPlantillaResponse> CrearAsync(EstadoPlantillaRequest request, CancellationToken cancellationToken = default)
    {
        var codigo = request.Codigo.Trim().ToUpperInvariant();
        if (await _db.EstadosPlantilla.AnyAsync(x => x.Codigo == codigo, cancellationToken))
            throw new ValidacionException($"Ya existe un estado con el código '{codigo}'.");

        var entity = new EstadoPlantilla
        {
            Codigo = codigo,
            Nombre = request.Nombre.Trim(),
            Orden = request.Orden,
            Activo = request.Activo,
        };
        _db.EstadosPlantilla.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("EstadoPlantilla creado Id {Id}", entity.EstadoPlantillaId);
        return ToResponse(entity);
    }

    public async Task<EstadoPlantillaResponse> ActualizarAsync(int id, EstadoPlantillaRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _db.EstadosPlantilla.FirstOrDefaultAsync(x => x.EstadoPlantillaId == id, cancellationToken);
        if (entity is null)
            throw new EntidadNoEncontradaException($"No existe el EstadoPlantilla con Id {id}.");

        var codigo = request.Codigo.Trim().ToUpperInvariant();
        if (await _db.EstadosPlantilla.AnyAsync(x => x.Codigo == codigo && x.EstadoPlantillaId != id, cancellationToken))
            throw new ValidacionException($"Ya existe otro estado con el código '{codigo}'.");

        entity.Codigo = codigo;
        entity.Nombre = request.Nombre.Trim();
        entity.Orden = request.Orden;
        entity.Activo = request.Activo;
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("EstadoPlantilla {Id} actualizado.", id);
        return ToResponse(entity);

    }

    private static EstadoPlantillaResponse ToResponse(EstadoPlantilla e) => new()
    {
        EstadoPlantillaId = e.EstadoPlantillaId,
        Codigo = e.Codigo,
        Nombre = e.Nombre,
        Orden = e.Orden,
        Activo = e.Activo,

    };
}