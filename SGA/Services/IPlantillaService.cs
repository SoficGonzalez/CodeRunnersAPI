using SGA.DTOs;

namespace SGA.Services;

public interface IPlantillaService
{
    Task<PlantillaResponse> CrearAsync(CrearPlantillaRequest request, CancellationToken cancellationToken = default);

    Task<ImportarPlantillaResponse> ImportarDocxAsync(
        Stream docxStream,
        string nombreArchivoOriginal,
        long tamanoBytes,
        string? descripcion,
        int estadoPlantillaId,
        int creadoPorUsuarioId,
        CancellationToken cancellationToken = default);
}
