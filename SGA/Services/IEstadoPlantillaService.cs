using SGA.DTOs;

namespace SGA.Services
{
    public interface IEstadoPlantillaService
    {
        Task<List<EstadoPlantillaResponse>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
        Task<EstadoPlantillaResponse> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
        Task<EstadoPlantillaResponse> CrearAsync(EstadoPlantillaRequest request, CancellationToken cancellationToken = default);
        Task<EstadoPlantillaResponse> ActualizarAsync(int id, EstadoPlantillaRequest request, CancellationToken cancellationToken = default);
    }
}
