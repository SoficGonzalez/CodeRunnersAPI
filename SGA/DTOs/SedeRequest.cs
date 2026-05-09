using System.ComponentModel.DataAnnotations;

namespace SGA.DTOs
{
    public class SedeRequest
    {
        [Required, MaxLength(120)]
        public string Nombre { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Direccion { get; set; } = string.Empty;

        [MaxLength(60)]
        public string? Departamento { get; set; }

        public bool Activa { get; set; } = true;
    }
}
