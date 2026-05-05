using System.ComponentModel.DataAnnotations;

namespace SGA.Models;

public class EstadoPlantilla
{
    public int EstadoPlantillaId { get; set; }

    [Required, MaxLength(20)]
    public string Codigo { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string Nombre { get; set; } = string.Empty;

    public int Orden { get; set; }

    public bool Activo { get; set; } = true;

    public ICollection<Plantilla> Plantillas { get; set; } = new List<Plantilla>();
}
