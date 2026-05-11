using SGA.DTOs;

namespace SGA.Services;

public interface IPlantillaService
{
    Task<PlantillaResponse> CrearAsync(
        CrearPlantillaRequest request,
        CancellationToken cancellationToken = default);

    Task<ImportarPlantillaResponse> ImportarDocxAsync(
        Stream docxStream,
        string nombreArchivoOriginal,
        long tamanoBytes,
        string? descripcion,
        int estadoPlantillaId,
        int creadoPorUsuarioId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<object>> GetAllAsync();

    Task<PlantillaResponse> ActualizarAsync(int plantillaId, ActualizarPlantillaRequest request, CancellationToken cancellationToken = default);
    Task<PlantillaResponse> ActualizarParcialAsync(
        int plantillaId,
        ActualizarParcialPlantillaRequest request,
        CancellationToken cancellationToken = default);

    Task<PlantillaDetalleResponse> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);
}
