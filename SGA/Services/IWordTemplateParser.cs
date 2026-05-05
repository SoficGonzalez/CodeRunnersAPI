using SGA.DTOs;

namespace SGA.Services;

public interface IWordTemplateParser
{
    /// <summary>
    /// Lee un archivo .docx y retorna la lista de campos (Content Controls) detectados.
    /// El orden refleja el orden de aparición en el documento. Los duplicados por ClaveCampo
    /// se descartan conservando la primera aparición.
    /// </summary>
    List<CampoDetectadoDto> ExtraerCampos(Stream docxStream);
}
