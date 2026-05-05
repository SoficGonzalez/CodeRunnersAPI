using System.ComponentModel.DataAnnotations;

namespace SGA.Models;

public class CampoPlantilla
{
    public int CampoPlantillaId { get; set; }

    public int PlantillaId { get; set; }
    public Plantilla Plantilla { get; set; } = null!;

    [Required, MaxLength(100)]
    public string ClaveCampo { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string EtiquetaPantalla { get; set; } = string.Empty;

    public int Orden { get; set; }

    public bool Obligatorio { get; set; }

    [Required, MaxLength(30)]
    public string TipoDato { get; set; } = "texto";

    [MaxLength(500)]
    public string? ValorPorDefecto { get; set; }

    public ICollection<ValorCampo> Valores { get; set; } = new List<ValorCampo>();
}
