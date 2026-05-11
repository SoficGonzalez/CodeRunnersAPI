using System.ComponentModel.DataAnnotations;

namespace SGA.DTOs;

public class EstadoPlantillaRequest
{
    [Required, MaxLength(20)]
    public string Codigo { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string Nombre { get; set; } = string.Empty;

    public int Orden { get; set; }

    public bool Activo { get; set; } = true;
}
