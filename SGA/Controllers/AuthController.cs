using Microsoft.AspNetCore.Mvc;
using SGA.DTOs;
using SGA.Services;
using Microsoft.AspNetCore.Authorization;

namespace SGA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;

        public AuthController(IAuthService service)
        {
            _service = service;
        }

        /// <summary>Login — retorna JWT</summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var result = await _service.LoginAsync(request, cancellationToken);
            return Ok(result);
        }

        /// <summary>Registrar usuario — solo Administrador</summary>
        [Authorize(Roles = "Administrador")]
        [HttpPost("registrar")]
        [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Registrar(
            [FromBody] RegistrarUsuarioRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var result = await _service.RegistrarAsync(request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, result);
        }
    }
}
