using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SGA.DTOs;

public class ImportarDocxRequest
{
    [Required]
    public IFormFile Archivo { get; set; } = default!;

    [Required, Range(1, int.MaxValue, ErrorMessage = "estadoPlantillaId debe ser mayor a 0.")]
    public int EstadoPlantillaId { get; set; }

    [Required, Range(1, int.MaxValue, ErrorMessage = "creadoPorUsuarioId debe ser mayor a 0.")]
    public int CreadoPorUsuarioId { get; set; }

    [MaxLength(500)]
    public string? Descripcion { get; set; }
}
