using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using SGA.DTOs;

namespace SGA.Services;

/// <summary>
/// Parser de plantillas Word (.docx) basado en DocumentFormat.OpenXml.
/// Detecta Content Controls (SDT) y produce una lista normalizada de campos.
///
/// La normalización de la clave se delega en <see cref="ClaveCampoNormalizer"/>
/// (compartida con el parser de RTF).
/// </summary>
public class WordTemplateParser : IWordTemplateParser
{
    public List<CampoDetectadoDto> ExtraerCampos(Stream docxStream)
    {
        ArgumentNullException.ThrowIfNull(docxStream);

        var resultado = new List<CampoDetectadoDto>();
        var clavesVistas = new HashSet<string>(StringComparer.Ordinal);

        using var doc = WordprocessingDocument.Open(docxStream, isEditable: false);
        var body = doc.MainDocumentPart?.Document?.Body;
        if (body is null)
        {
            return resultado;
        }

        var sdtElements = body.Descendants<SdtElement>().ToList();

        var orden = 0;
        var indiceFallback = 0;
        foreach (var sdt in sdtElements)
        {
            indiceFallback++;
            var props = sdt.SdtProperties;
            var tag = props?.GetFirstChild<Tag>()?.Val?.Value;
            var alias = props?.GetFirstChild<SdtAlias>()?.Val?.Value;

            var rawKey = !string.IsNullOrWhiteSpace(tag)
                ? tag!
                : (!string.IsNullOrWhiteSpace(alias) ? alias! : $"campo_{indiceFallback}");

            var claveNormalizada = ClaveCampoNormalizer.Normalizar(rawKey, indiceFallback);
            if (!clavesVistas.Add(claveNormalizada))
            {
                continue;
            }

            var etiqueta = !string.IsNullOrWhiteSpace(alias)
                ? alias!.Trim()
                : (!string.IsNullOrWhiteSpace(tag) ? tag!.Trim() : $"Campo {indiceFallback}");

            if (etiqueta.Length > 150)
            {
                etiqueta = etiqueta[..150];
            }

            orden++;
            resultado.Add(new CampoDetectadoDto
            {
                FieldKey = claveNormalizada,
                DisplayLabel = etiqueta,
                Orden = orden
            });
        }

        return resultado;
    }
}
