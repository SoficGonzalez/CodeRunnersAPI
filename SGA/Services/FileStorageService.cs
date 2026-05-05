using Microsoft.Extensions.Options;
using SGA.Configuration;

namespace SGA.Services;

public class FileStorageService : IFileStorageService
{
    private static readonly string[] ExtensionesSoportadas = { ".docx", ".rtf" };

    private readonly StorageOptions _options;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(IOptions<StorageOptions> options, ILogger<FileStorageService> logger)
    {
        _options = options.Value;
        _logger = logger;

        if (!Directory.Exists(_options.RutaPlantillas))
        {
            Directory.CreateDirectory(_options.RutaPlantillas);
            _logger.LogInformation("Carpeta de plantillas creada: {Ruta}", _options.RutaPlantillas);
        }
    }

    public async Task<string> GuardarPlantillaAsync(int plantillaId, Stream contenido, string extension, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contenido);

        var rutaDestino = ConstruirRutaPlantilla(plantillaId, extension);
        var directorio = Path.GetDirectoryName(rutaDestino);
        if (!string.IsNullOrEmpty(directorio) && !Directory.Exists(directorio))
        {
            Directory.CreateDirectory(directorio);
        }

        if (contenido.CanSeek)
        {
            contenido.Position = 0;
        }

        await using (var fs = new FileStream(rutaDestino, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await contenido.CopyToAsync(fs, cancellationToken);
        }

        _logger.LogInformation("Plantilla {PlantillaId} almacenada en {Ruta}", plantillaId, rutaDestino);
        return rutaDestino;
    }

    public void EliminarPlantillaSiExiste(int plantillaId)
    {
        foreach (var ext in ExtensionesSoportadas)
        {
            var ruta = ConstruirRutaPlantilla(plantillaId, ext);
            try
            {
                if (File.Exists(ruta))
                {
                    File.Delete(ruta);
                    _logger.LogWarning("Plantilla {PlantillaId} eliminada físicamente: {Ruta}", plantillaId, ruta);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo eliminar el archivo físico de la plantilla {PlantillaId}", plantillaId);
            }
        }
    }

    public string ConstruirRutaPlantilla(int plantillaId, string extension)
    {
        var ext = NormalizarExtension(extension);
        return Path.Combine(_options.RutaPlantillas, $"{plantillaId}{ext}");
    }

    private static string NormalizarExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return ".docx";
        }
        var ext = extension.Trim().ToLowerInvariant();
        if (!ext.StartsWith('.'))
        {
            ext = "." + ext;
        }
        return ext;
    }
}
