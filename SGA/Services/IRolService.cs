using SGA.DTOs;

namespace SGA.Services
{
    public interface IRolService
    {
        Task<List<RolResponse>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
        Task<RolResponse> ObtenerPorIdAsync(int rolId, CancellationToken cancellationToken = default);
        Task<RolResponse> CrearAsync(RolRequest request, CancellationToken cancellationToken = default);
        Task<RolResponse> ActualizarAsync(int rolId, RolRequest request, CancellationToken cancellationToken = default);
    }
}
