using System.ComponentModel.DataAnnotations;

namespace SGA.DTOs;

public class CrearPlantillaRequest
{
    [Required, MaxLength(180)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    [Required, Range(1, int.MaxValue, ErrorMessage = "estadoPlantillaId debe ser mayor a 0.")]
    public int EstadoPlantillaId { get; set; }

    [Required, Range(1, int.MaxValue, ErrorMessage = "creadoPorUsuarioId debe ser mayor a 0.")]
    public int CreadoPorUsuarioId { get; set; }
}
