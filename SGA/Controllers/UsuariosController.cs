using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SGA.DTOs;
using SGA.Services;

namespace SGA.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _service;
        public UsuariosController(IUsuarioService service)
        {
            _service = service;
        }
        [HttpGet]
        [ProducesResponseType(typeof(List<UsuarioResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerTodos(CancellationToken cancellationToken)
        {
            var list = await _service.ObtenerTodosAsync(cancellationToken);
            return Ok(list);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerPorId(int id, CancellationToken cancellationToken)
        {
            var u = await _service.ObtenerPorIdAsync(id, cancellationToken);
            return Ok(u);
        }

        [HttpPost]
        [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Crear([FromBody] UsuarioRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var creado = await _service.CrearAsync(request, cancellationToken);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.UsuarioId }, creado);
        }
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Actualizar(int id, [FromBody] UsuarioRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var actualizado = await _service.ActualizarAsync(id, request, cancellationToken);
            return Ok(actualizado);
        }


    }
}
