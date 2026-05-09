using System.ComponentModel.DataAnnotations;

namespace SGA.DTOs
{
    public class UsuarioRequest
    {
        [Required, MaxLength(60)]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required, MaxLength(150), EmailAddress]
        public string Correo { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int RolId { get; set; }

        public int? SedeId { get; set; }

        public bool Activo { get; set; } = true;
    }
}
