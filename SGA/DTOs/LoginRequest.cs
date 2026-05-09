using System.ComponentModel.DataAnnotations;

namespace SGA.DTOs
{
    public class LoginRequest
    {
        [Required, EmailAddress]
        public string Correo { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
