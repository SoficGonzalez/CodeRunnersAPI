using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SGA.Configuration;
using SGA.DTOs;
using SGA.Services;

namespace SGA.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PlantillasController : ControllerBase
{
    private readonly IPlantillaService _service;
    private readonly StorageOptions _storageOptions;
    private readonly ILogger<PlantillasController> _logger;

    public PlantillasController(
        IPlantillaService service,
        IOptions<StorageOptions> storageOptions,
        ILogger<PlantillasController> logger)
    {
        _service = service;
        _storageOptions = storageOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// Crea una plantilla manual (solo metadata, sin importar archivo).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PlantillaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Crear(
        [FromBody] CrearPlantillaRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var resp = await _service.CrearAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Crear), new { id = resp.PlantillaId }, resp);
    }

    /// <summary>
    /// Importa una plantilla a partir de un archivo de Word (.docx) o RTF (.rtf).
    /// - Para .docx detecta los Content Controls (SDT) del documento.
    /// - Para .rtf detecta marcadores de texto del tipo {{nombre_campo}} o &lt;&lt;nombre_campo&gt;&gt;.
    /// </summary>
    [HttpPost("importar-docx")]
    [Consumes("multipart/form-data")]
    [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
    [RequestSizeLimit(long.MaxValue)]
    [ProducesResponseType(typeof(ImportarPlantillaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ImportarDocx(
        [FromForm] ImportarDocxRequest form,
        CancellationToken cancellationToken)
    {
        if (form is null || form.Archivo is null || form.Archivo.Length == 0)
        {
            throw new ValidacionException("El archivo es obligatorio y no puede estar vacío.");
        }

        if (form.Archivo.Length > _storageOptions.TamanoMaximoBytes)
        {
            throw new ValidacionException(
                $"El archivo supera el tamaño máximo permitido de {_storageOptions.TamanoMaximoMB} MB.");
        }

        if (form.EstadoPlantillaId <= 0)
        {
            throw new ValidacionException("estadoPlantillaId es obligatorio y debe ser mayor a 0.");
        }

        if (form.CreadoPorUsuarioId <= 0)
        {
            throw new ValidacionException("creadoPorUsuarioId es obligatorio y debe ser mayor a 0.");
        }

        var extension = Path.GetExtension(form.Archivo.FileName);
        if (!string.Equals(extension, ".docx", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(extension, ".rtf", StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidacionException("Solo se permiten archivos con extensión .docx o .rtf.");
        }

        await using var stream = form.Archivo.OpenReadStream();
        var resp = await _service.ImportarDocxAsync(
            stream,
            form.Archivo.FileName,
            form.Archivo.Length,
            form.Descripcion,
            form.EstadoPlantillaId,
            form.CreadoPorUsuarioId,
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, resp);
    }
}
