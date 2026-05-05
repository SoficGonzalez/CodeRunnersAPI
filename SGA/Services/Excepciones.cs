namespace SGA.Services;

/// <summary>
/// Recurso referenciado no existe en BD (estado, usuario, plantilla, etc.).
/// Mapea a 404 Not Found.
/// </summary>
public class EntidadNoEncontradaException : Exception
{
    public EntidadNoEncontradaException(string mensaje) : base(mensaje) { }
}

/// <summary>
/// Validación de regla de negocio (extensión, tamaño, archivo vacío, etc.).
/// Mapea a 400 Bad Request.
/// </summary>
public class ValidacionException : Exception
{
    public ValidacionException(string mensaje) : base(mensaje) { }
}

/// <summary>
/// El docx fue procesado pero no contiene Content Controls dinámicos.
/// Mapea a 422 Unprocessable Entity.
/// </summary>
public class SinCamposDetectadosException : Exception
{
    public SinCamposDetectadosException(string mensaje) : base(mensaje) { }
}
