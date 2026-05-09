using SGA.DTOs;

namespace SGA.Services
{
    public interface IUsuarioService
    {
        Task<List<UsuarioResponse>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
        Task<UsuarioResponse> ObtenerPorIdAsync(int usuarioId, CancellationToken cancellationToken = default);
        Task<UsuarioResponse> CrearAsync(UsuarioRequest request, CancellationToken cancellationToken = default);
        Task<UsuarioResponse> ActualizarAsync(int usuarioId, UsuarioRequest request, CancellationToken cancellationToken = default);
    }
}
