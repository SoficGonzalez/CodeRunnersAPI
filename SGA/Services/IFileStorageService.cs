namespace SGA.Services;

public interface IFileStorageService
{
    /// <summary>
    /// Persiste el contenido de un stream como archivo de plantilla.
    /// El nombre físico es <c>{plantillaId}{extension}</c> (extensión incluyendo el punto, p. ej. ".docx" o ".rtf").
    /// Retorna la ruta completa donde se guardó el archivo.
    /// </summary>
    Task<string> GuardarPlantillaAsync(int plantillaId, Stream contenido, string extension, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina cualquier archivo físico de la plantilla (cualquier extensión soportada), si existe.
    /// </summary>
    void EliminarPlantillaSiExiste(int plantillaId);

    /// <summary>
    /// Construye la ruta absoluta esperada para una plantilla con la extensión dada (sin verificar existencia).
    /// </summary>
    string ConstruirRutaPlantilla(int plantillaId, string extension);
}
