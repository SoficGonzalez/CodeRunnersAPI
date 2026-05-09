using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SGA.Configuration;
using SGA.Data;
using SGA.DTOs;
using SGA.Models;

namespace SGA.Services;

public class PlantillaService : IPlantillaService
{
    private static readonly HashSet<string> ExtensionesSoportadas =
        new(StringComparer.OrdinalIgnoreCase) { ".docx", ".rtf" };

    private readonly SgaDbContext _db;
    private readonly IWordTemplateParser _docxParser;
    private readonly IRtfTemplateParser _rtfParser;
    private readonly IFileStorageService _storage;
    private readonly StorageOptions _storageOptions;
    private readonly ILogger<PlantillaService> _logger;

    public PlantillaService(
        SgaDbContext db,
        IWordTemplateParser docxParser,
        IRtfTemplateParser rtfParser,
        IFileStorageService storage,
        IOptions<StorageOptions> storageOptions,
        ILogger<PlantillaService> logger)
    {
        _db = db;
        _docxParser = docxParser;
        _rtfParser = rtfParser;
        _storage = storage;
        _storageOptions = storageOptions.Value;
        _logger = logger;
    }

    public async Task<PlantillaResponse> CrearAsync(CrearPlantillaRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await ValidarEstadoYUsuarioAsync(request.EstadoPlantillaId, request.CreadoPorUsuarioId, cancellationToken);

        var ahora = DateTime.UtcNow;
        var plantilla = new Plantilla
        {
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion?.Trim(),
            EstadoPlantillaId = request.EstadoPlantillaId,
            CreadoPorUsuarioId = request.CreadoPorUsuarioId,
            Activa = true,
            FechaCreacion = ahora,
            FechaActualizacion = ahora,
        };

        _db.Plantillas.Add(plantilla);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Plantilla manual creada con Id {PlantillaId}", plantilla.PlantillaId);

        return new PlantillaResponse
        {
            PlantillaId = plantilla.PlantillaId,
            Nombre = plantilla.Nombre,
            Descripcion = plantilla.Descripcion,
            EstadoPlantillaId = plantilla.EstadoPlantillaId,
            CreadoPorUsuarioId = plantilla.CreadoPorUsuarioId,
            StoragePath = plantilla.RutaArchivoWord,
            FechaCreacion = plantilla.FechaCreacion,
        };
    }

    public async Task<ImportarPlantillaResponse> ImportarDocxAsync(
        Stream docxStream,
        string nombreArchivoOriginal,
        long tamanoBytes,
        string? descripcion,
        int estadoPlantillaId,
        int creadoPorUsuarioId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(docxStream);

        if (tamanoBytes <= 0)
        {
            throw new ValidacionException("El archivo está vacío.");
        }

        if (tamanoBytes > _storageOptions.TamanoMaximoBytes)
        {
            throw new ValidacionException(
                $"El archivo supera el tamaño máximo permitido de {_storageOptions.TamanoMaximoMB} MB.");
        }

        if (string.IsNullOrWhiteSpace(nombreArchivoOriginal))
        {
            throw new ValidacionException("El nombre del archivo es obligatorio.");
        }

        var extension = (Path.GetExtension(nombreArchivoOriginal) ?? string.Empty).ToLowerInvariant();
        if (!ExtensionesSoportadas.Contains(extension))
        {
            throw new ValidacionException("Solo se permiten archivos con extensión .docx o .rtf.");
        }

        await ValidarEstadoYUsuarioAsync(estadoPlantillaId, creadoPorUsuarioId, cancellationToken);

        // 1) Materializar el stream entrante en memoria para poder parsearlo y luego copiarlo a disco.
        await using var memoria = new MemoryStream();
        if (docxStream.CanSeek)
        {
            docxStream.Position = 0;
        }
        await docxStream.CopyToAsync(memoria, cancellationToken);
        memoria.Position = 0;

        // 2) Parsear campos según extensión.
        List<CampoDetectadoDto> campos;
        try
        {
            campos = extension switch
            {
                ".docx" => _docxParser.ExtraerCampos(memoria),
                ".rtf" => _rtfParser.ExtraerCampos(memoria),
                _ => throw new ValidacionException("Extensión no soportada."),
            };
        }
        catch (ValidacionException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al parsear el archivo {Extension}", extension);
            throw new InvalidOperationException($"No se pudo procesar el archivo {extension}.", ex);
        }

        if (campos.Count == 0)
        {
            var msg = extension == ".docx"
                ? "El documento no contiene Content Controls (controles de contenido) detectables."
                : "El documento no contiene marcadores detectables (use {{nombre_campo}} o <<nombre_campo>>).";
            throw new SinCamposDetectadosException(msg);
        }

        // 3) Insertar plantilla y campos dentro de una transacción y luego copiar a disco.
        //    Como el DbContext está configurado con EnableRetryOnFailure(), no podemos
        //    usar BeginTransactionAsync directamente: EF Core obliga a envolver el bloque
        //    transaccional en un IExecutionStrategy.ExecuteAsync para que el reintento
        //    pueda repetir la unidad completa de forma idempotente.
        var ahora = DateTime.UtcNow;
        var nombrePlantilla = Path.GetFileName(nombreArchivoOriginal);

        var strategy = _db.Database.CreateExecutionStrategy();

        var resultado = await strategy.ExecuteAsync(async () =>
        {
            // Limpiamos el ChangeTracker para que un eventual reintento no choque con
            // las entidades ya tracked en el intento anterior.
            _db.ChangeTracker.Clear();

            await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);

            var plantilla = new Plantilla
            {
                Nombre = nombrePlantilla,
                Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion!.Trim(),
                EstadoPlantillaId = estadoPlantillaId,
                CreadoPorUsuarioId = creadoPorUsuarioId,
                Activa = true,
                FechaCreacion = ahora,
                FechaActualizacion = ahora,
            };

            _db.Plantillas.Add(plantilla);
            await _db.SaveChangesAsync(cancellationToken);

            foreach (var c in campos)
            {
                _db.CamposPlantilla.Add(new CampoPlantilla
                {
                    PlantillaId = plantilla.PlantillaId,
                    ClaveCampo = c.FieldKey,
                    EtiquetaPantalla = c.DisplayLabel,
                    Orden = c.Orden,
                    Obligatorio = false,
                    TipoDato = "texto",
                    ValorPorDefecto = null,
                });
            }

            string ruta;
            try
            {
                memoria.Position = 0;
                ruta = await _storage.GuardarPlantillaAsync(plantilla.PlantillaId, memoria, extension, cancellationToken);
                plantilla.RutaArchivoWord = ruta;

                await _db.SaveChangesAsync(cancellationToken);
                await tx.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error guardando archivo físico de plantilla, rollback transaccional");
                await tx.RollbackAsync(cancellationToken);
                _storage.EliminarPlantillaSiExiste(plantilla.PlantillaId);
                throw;
            }

            return new
            {
                plantilla.PlantillaId,
                plantilla.Nombre,
                plantilla.Descripcion,
                plantilla.EstadoPlantillaId,
                plantilla.CreadoPorUsuarioId,
                RutaGuardada = ruta,
                plantilla.FechaCreacion,
            };
        });

        _logger.LogInformation(
            "Plantilla {PlantillaId} ({Extension}) importada con {NumCampos} campos detectados",
            resultado.PlantillaId, extension, campos.Count);

        return new ImportarPlantillaResponse
        {
            PlantillaId = resultado.PlantillaId,
            Nombre = resultado.Nombre,
            Descripcion = resultado.Descripcion,
            EstadoPlantillaId = resultado.EstadoPlantillaId,
            CreadoPorUsuarioId = resultado.CreadoPorUsuarioId,
            StoragePath = resultado.RutaGuardada,
            CamposDetectados = campos,
            FechaCreacion = resultado.FechaCreacion,
        };
    }

    private async Task ValidarEstadoYUsuarioAsync(int estadoPlantillaId, int usuarioId, CancellationToken cancellationToken)
    {
        var estadoExiste = await _db.EstadosPlantilla
            .AsNoTracking()
            .AnyAsync(e => e.EstadoPlantillaId == estadoPlantillaId, cancellationToken);
        if (!estadoExiste)
        {
            throw new EntidadNoEncontradaException(
                $"No existe el EstadoPlantilla con Id {estadoPlantillaId}.");
        }

        var usuarioExiste = await _db.Usuarios
            .AsNoTracking()
            .AnyAsync(u => u.UsuarioId == usuarioId, cancellationToken);
        if (!usuarioExiste)
        {
            throw new EntidadNoEncontradaException(
                $"No existe el Usuario con Id {usuarioId}.");
        }
    }
public async Task<IEnumerable<object>> GetAllAsync()
{
    var plantillas = await _db.Plantillas
        .Include(p => p.EstadoPlantilla)
        .Select(p => new
        {
            plantillaId = p.PlantillaId,
            nombre = p.Nombre,
            descripcion = p.Descripcion,
            estadoActual = p.EstadoPlantilla.Nombre,
            fechaCreacion = p.FechaCreacion,
            activa = p.Activa
        })
        .ToListAsync();

    return plantillas;
}
}
