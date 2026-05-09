using System.ComponentModel.DataAnnotations;

namespace SGA.DTOs
{
    public class RolRequest
    {
        [Required, MaxLength(60)]
        public string NombreRol { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;
    }
}
