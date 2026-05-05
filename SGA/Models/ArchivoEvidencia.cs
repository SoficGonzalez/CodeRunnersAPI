using System.ComponentModel.DataAnnotations;

namespace SGA.Models;

public class ArchivoEvidencia
{
    public int ArchivoEvidenciaId { get; set; }

    public int DocumentoLlenadoId { get; set; }
    public DocumentoLlenado DocumentoLlenado { get; set; } = null!;

    public int SubidoPorUsuarioId { get; set; }
    public Usuario SubidoPor { get; set; } = null!;

    [Required, MaxLength(255)]
    public string NombreArchivo { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string RutaEnRepositorio { get; set; } = string.Empty;

    [MaxLength(120)]
    public string? TipoContenido { get; set; }

    public long TamanoBytes { get; set; }

    public DateTime FechaSubida { get; set; } = DateTime.UtcNow;
}
