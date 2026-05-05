using System.Text.Json;
using SGA.DTOs;
using SGA.Services;

namespace SGA.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidacionException ex)
        {
            await EscribirRespuestaAsync(context, StatusCodes.Status400BadRequest, "Validación fallida", ex.Message);
        }
        catch (EntidadNoEncontradaException ex)
        {
            await EscribirRespuestaAsync(context, StatusCodes.Status404NotFound, "Recurso no encontrado", ex.Message);
        }
        catch (SinCamposDetectadosException ex)
        {
            await EscribirRespuestaAsync(context, StatusCodes.Status422UnprocessableEntity, "Sin campos detectados", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado en {Method} {Path}", context.Request.Method, context.Request.Path);
            var detalle = _env.IsDevelopment()
                ? $"{ex.Message}{Environment.NewLine}{ex.StackTrace}"
                : "Ocurrió un error procesando la solicitud.";
            await EscribirRespuestaAsync(context, StatusCodes.Status500InternalServerError, "Error interno del servidor", detalle);
        }
    }

    private static async Task EscribirRespuestaAsync(HttpContext context, int statusCode, string title, string detail)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";

        var body = new ErrorResponse
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            TraceId = context.TraceIdentifier,
        };

        await JsonSerializer.SerializeAsync(context.Response.Body, body, JsonOpts);
    }
}
