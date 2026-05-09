namespace SGA.DTOs
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }
}
