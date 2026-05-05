using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using SGA.DTOs;

namespace SGA.Services;

/// <summary>
/// Parser de plantillas RTF.
///
/// Estrategia:
///   1) Lee el RTF y lo convierte a texto plano con un decodificador minimalista
///      (maneja grupos {...}, control words, escapes \{ \} \\, hex \'XX,
///      unicode \uNNNN y destinos ignorables como fonttbl/colortbl/info/pict).
///   2) Sobre el texto plano busca marcadores tipo {{clave}} o &lt;&lt;clave&gt;&gt;.
///   3) Normaliza la clave con <see cref="ClaveCampoNormalizer"/> y dedupe.
///
/// Limitaciones conocidas:
///   - No interpreta tablas de colores ni fuentes; sólo extrae texto.
///   - Los marcadores deben quedar contiguos en el texto (no romper la palabra
///     con cambios de formato a mitad de la clave). Si Word divide el texto en
///     runs distintos pero sin destinos ignorables, el extractor concatena bien.
/// </summary>
public class RtfTemplateParser : IRtfTemplateParser
{
    private static readonly Regex PlaceholderRegex = new(
        @"\{\{\s*(?<key1>[^\{\}\r\n]{1,200}?)\s*\}\}|<<\s*(?<key2>[^<>\r\n]{1,200}?)\s*>>",
        RegexOptions.Compiled);

    private static readonly HashSet<string> DestinosIgnorables = new(StringComparer.Ordinal)
    {
        "fonttbl", "colortbl", "stylesheet", "filetbl", "listtable",
        "listoverridetable", "rsidtbl", "info", "pict", "object",
        "themedata", "datastore", "wgrffmtfilter", "latentstyles",
        "lsdlockedexcept", "generator",
        "header", "footer", "headerl", "headerr", "headerf", "footerl", "footerr", "footerf",
        "fldinst",
        "xmlnstbl", "operator", "author", "title", "subject",
        "keywords", "comment", "doccomm",
    };

    public List<CampoDetectadoDto> ExtraerCampos(Stream rtfStream)
    {
        ArgumentNullException.ThrowIfNull(rtfStream);

        if (rtfStream.CanSeek)
        {
            rtfStream.Position = 0;
        }

        // RTF es ASCII-compatible; los bytes >127 vienen como hex escapes \'XX
        // o como \uNNNN, así que leer como ISO-8859-1 (Latin-1) preserva todo
        // sin perder bytes en la conversión.
        using var reader = new StreamReader(rtfStream, Encoding.GetEncoding("ISO-8859-1"), detectEncodingFromByteOrderMarks: false, bufferSize: 4096, leaveOpen: true);
        var rtf = reader.ReadToEnd();

        var plainText = ConvertirRtfATextoPlano(rtf);

        var resultado = new List<CampoDetectadoDto>();
        var clavesVistas = new HashSet<string>(StringComparer.Ordinal);

        var orden = 0;
        var indiceFallback = 0;
        foreach (Match m in PlaceholderRegex.Matches(plainText))
        {
            indiceFallback++;
            var rawKey = (m.Groups["key1"].Success ? m.Groups["key1"].Value : m.Groups["key2"].Value).Trim();

            var clave = ClaveCampoNormalizer.Normalizar(rawKey, indiceFallback);
            if (!clavesVistas.Add(clave))
            {
                continue;
            }

            var etiqueta = string.IsNullOrWhiteSpace(rawKey)
                ? $"Campo {indiceFallback}"
                : (rawKey.Length > 150 ? rawKey[..150] : rawKey);

            orden++;
            resultado.Add(new CampoDetectadoDto
            {
                FieldKey = clave,
                DisplayLabel = etiqueta,
                Orden = orden,
            });
        }

        return resultado;
    }

    /// <summary>
    /// Convierte texto RTF a texto plano. Implementación deliberadamente simple,
    /// suficiente para detectar marcadores de campos. NO pretende ser un
    /// renderizador completo de RTF.
    /// </summary>
    public static string ConvertirRtfATextoPlano(string rtf)
    {
        if (string.IsNullOrEmpty(rtf))
        {
            return string.Empty;
        }

        var win1252 = Encoding.GetEncoding(1252);
        var sb = new StringBuilder(rtf.Length);

        var ignoreStack = new Stack<bool>();
        bool ignorando = false;
        bool destinoOpcionalPendiente = false;

        int i = 0;
        int n = rtf.Length;

        while (i < n)
        {
            char c = rtf[i];

            if (c == '{')
            {
                ignoreStack.Push(ignorando);
                i++;
                continue;
            }

            if (c == '}')
            {
                if (ignoreStack.Count > 0)
                {
                    ignorando = ignoreStack.Pop();
                }
                destinoOpcionalPendiente = false;
                i++;
                continue;
            }

            if (c == '\\')
            {
                i++;
                if (i >= n) break;
                char siguiente = rtf[i];

                if (siguiente == '\\' || siguiente == '{' || siguiente == '}')
                {
                    if (!ignorando) sb.Append(siguiente);
                    i++;
                    continue;
                }

                if (siguiente == '\'')
                {
                    if (i + 2 < n)
                    {
                        var hex = rtf.Substring(i + 1, 2);
                        if (byte.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var b))
                        {
                            if (!ignorando)
                            {
                                sb.Append(win1252.GetString(new[] { b }));
                            }
                        }
                        i += 3;
                    }
                    else
                    {
                        i++;
                    }
                    continue;
                }

                if (siguiente == '*')
                {
                    destinoOpcionalPendiente = true;
                    i++;
                    continue;
                }

                if (siguiente == '\r' || siguiente == '\n')
                {
                    if (!ignorando) sb.Append('\n');
                    i++;
                    continue;
                }

                if (char.IsLetter(siguiente))
                {
                    int inicio = i;
                    while (i < n && char.IsLetter(rtf[i])) i++;
                    var palabra = rtf.Substring(inicio, i - inicio);

                    int paramInicio = i;
                    if (i < n && (rtf[i] == '-' || char.IsDigit(rtf[i])))
                    {
                        if (rtf[i] == '-') i++;
                        while (i < n && char.IsDigit(rtf[i])) i++;
                    }
                    bool tieneParam = i > paramInicio;
                    int valorParam = 0;
                    if (tieneParam)
                    {
                        int.TryParse(rtf.AsSpan(paramInicio, i - paramInicio),
                            NumberStyles.Integer, CultureInfo.InvariantCulture, out valorParam);
                    }

                    if (i < n && rtf[i] == ' ') i++;

                    if (destinoOpcionalPendiente)
                    {
                        ignorando = true;
                        destinoOpcionalPendiente = false;
                        continue;
                    }

                    if (palabra is "par" or "line" or "sect")
                    {
                        if (!ignorando) sb.Append('\n');
                    }
                    else if (palabra == "tab")
                    {
                        if (!ignorando) sb.Append('\t');
                    }
                    else if (palabra == "u" && tieneParam)
                    {
                        int code = valorParam;
                        if (code < 0) code += 65536;
                        if (!ignorando && code > 0 && code <= 0x10FFFF)
                        {
                            try
                            {
                                sb.Append(char.ConvertFromUtf32(code));
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                // Code point inválido, lo ignoramos.
                            }
                        }
                        if (i < n)
                        {
                            if (rtf[i] == '\\' && i + 3 < n && rtf[i + 1] == '\'')
                            {
                                i += 4;
                            }
                            else
                            {
                                i++;
                            }
                        }
                    }
                    else if (DestinosIgnorables.Contains(palabra))
                    {
                        ignorando = true;
                    }

                    continue;
                }

                i++;
                continue;
            }

            if (c == '\r' || c == '\n')
            {
                i++;
                continue;
            }

            if (!ignorando)
            {
                sb.Append(c);
            }
            i++;
        }

        return sb.ToString();
    }
}
