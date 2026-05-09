using Microsoft.AspNetCore.Mvc;
using SGA.DTOs;
using SGA.Services;

namespace SGA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class SedesController : ControllerBase
    {
        private readonly ISedeService _service;

        public SedesController(ISedeService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<SedeResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerTodas(CancellationToken cancellationToken)
        {
            var result = await _service.ObtenerTodasAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(SedeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerPorId(int id, CancellationToken cancellationToken)
        {
            var result = await _service.ObtenerPorIdAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(SedeResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Crear([FromBody] SedeRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var result = await _service.CrearAsync(request, cancellationToken);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = result.SedeId }, result);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(SedeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Actualizar(int id, [FromBody] SedeRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var result = await _service.ActualizarAsync(id, request, cancellationToken);
            return Ok(result);
        }
    }
}
