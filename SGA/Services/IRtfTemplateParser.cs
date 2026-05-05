using SGA.DTOs;

namespace SGA.Services;

/// <summary>
/// Lee un archivo .rtf desde un stream y retorna la lista de campos detectados.
/// Detecta marcadores de la forma <c>{{nombre_campo}}</c> o
/// <c>&lt;&lt;nombre_campo&gt;&gt;</c> en el texto del documento.
/// El orden refleja el orden de aparición; los duplicados por ClaveCampo se
/// descartan conservando la primera aparición.
/// </summary>
public interface IRtfTemplateParser
{
    List<CampoDetectadoDto> ExtraerCampos(Stream rtfStream);
}
