using System.ComponentModel.DataAnnotations;

namespace SGA.Models;

public class Plantilla
{
    public int PlantillaId { get; set; }

    public int EstadoPlantillaId { get; set; }
    public EstadoPlantilla EstadoPlantilla { get; set; } = null!;

    public int CreadoPorUsuarioId { get; set; }
    public Usuario CreadoPor { get; set; } = null!;

    [Required, MaxLength(180)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    [MaxLength(500)]
    public string? RutaArchivoWord { get; set; }

    public bool Activa { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

    public ICollection<CampoPlantilla> Campos { get; set; } = new List<CampoPlantilla>();
    public ICollection<DocumentoLlenado> Llenados { get; set; } = new List<DocumentoLlenado>();
}
