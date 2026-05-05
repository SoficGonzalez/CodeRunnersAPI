using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SGA.Services;

/// <summary>
/// Helper compartido para normalizar la <c>ClaveCampo</c> (fieldKey) de un campo
/// de plantilla, sin importar si proviene de un .docx (Content Control) o de un
/// .rtf (placeholder {{...}} / &lt;&lt;...&gt;&gt;).
///
/// Reglas:
///   1) trim, ToLowerInvariant, eliminar acentos (NFD).
///   2) espacios → "_".
///   3) eliminar todo lo que no sea [a-z0-9_].
///   4) colapsar "_" repetidos y recortar guiones de los extremos.
///   5) máx 100 chars; si queda vacío → "campo_{indiceFallback}".
/// </summary>
public static class ClaveCampoNormalizer
{
    private static readonly Regex NoAlfaNum = new("[^a-z0-9_]+", RegexOptions.Compiled);
    private static readonly Regex MultiUnderscore = new("_{2,}", RegexOptions.Compiled);

    public static string Normalizar(string entrada, int indiceFallback)
    {
        if (string.IsNullOrWhiteSpace(entrada))
        {
            return $"campo_{indiceFallback}";
        }

        var sinAcentos = QuitarAcentos(entrada.Trim());
        var lower = sinAcentos.ToLowerInvariant();
        var conGuion = lower.Replace(' ', '_');
        var soloAlfaNum = NoAlfaNum.Replace(conGuion, "_");
        var colapsado = MultiUnderscore.Replace(soloAlfaNum, "_").Trim('_');

        if (string.IsNullOrEmpty(colapsado))
        {
            return $"campo_{indiceFallback}";
        }

        if (colapsado.Length > 100)
        {
            colapsado = colapsado[..100].TrimEnd('_');
            if (string.IsNullOrEmpty(colapsado))
            {
                return $"campo_{indiceFallback}";
            }
        }

        return colapsado;
    }

    private static string QuitarAcentos(string texto)
    {
        var formaD = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(formaD.Length);
        foreach (var c in formaD)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(c);
            if (uc != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
