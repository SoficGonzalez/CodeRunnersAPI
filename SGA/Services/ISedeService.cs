using SGA.DTOs;

namespace SGA.Services
{
    public interface ISedeService
    {
        Task<List<SedeResponse>> ObtenerTodasAsync(CancellationToken cancellationToken = default);
        Task<SedeResponse> ObtenerPorIdAsync(int sedeId, CancellationToken cancellationToken = default);
        Task<SedeResponse> CrearAsync(SedeRequest request, CancellationToken cancellationToken = default);
        Task<SedeResponse> ActualizarAsync(int sedeId, SedeRequest request, CancellationToken cancellationToken = default);
    }
}
