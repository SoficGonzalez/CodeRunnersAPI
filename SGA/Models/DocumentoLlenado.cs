using System.ComponentModel.DataAnnotations;

namespace SGA.Models;

public class DocumentoLlenado
{
    public int DocumentoLlenadoId { get; set; }

    public int PlantillaId { get; set; }
    public Plantilla Plantilla { get; set; } = null!;

    public int RegistradoPorUsuarioId { get; set; }
    public Usuario RegistradoPor { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Titulo { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? RutaDocumentoGenerado { get; set; }

    [MaxLength(1000)]
    public string? Notas { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public ICollection<ValorCampo> Valores { get; set; } = new List<ValorCampo>();
    public ICollection<ArchivoEvidencia> Evidencias { get; set; } = new List<ArchivoEvidencia>();
}
