using System.ComponentModel.DataAnnotations;

namespace SGA.Models
{
    public class Sede
    {
        public int SedeId { get; set; }

        [Required, MaxLength(120)]
        public string Nombre { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Direccion { get; set; } = string.Empty;

        [MaxLength(60)]
        public string? Departamento { get; set; }

        public bool Activa { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}
