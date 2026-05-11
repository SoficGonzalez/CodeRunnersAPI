using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SGA.DTOs;
using SGA.Services;

namespace SGA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class RolesController : ControllerBase
    {
        private readonly IRolService _service;
        public RolesController(IRolService service)
        {
            _service = service;
        }
        [HttpGet]
        [Authorize(Roles = "Administrador,RRHH,Operador")]
        [ProducesResponseType(typeof(List<RolResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerTodos(CancellationToken cancellationToken)
        {
            return Ok(await _service.ObtenerTodosAsync(cancellationToken));
        }
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Administrador,RRHH,Operador")]
        [ProducesResponseType(typeof(RolResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerPorId(int id, CancellationToken cancellationToken)
        {
            return Ok(await _service.ObtenerPorIdAsync(id, cancellationToken));
        }
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ProducesResponseType(typeof(RolResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Crear([FromBody] RolRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var creado = await _service.CrearAsync(request, cancellationToken);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.RolId }, creado);
        }
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Administrador")]
        [ProducesResponseType(typeof(RolResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Actualizar(int id, [FromBody] RolRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            return Ok(await _service.ActualizarAsync(id, request, cancellationToken));
        }


    }
}
