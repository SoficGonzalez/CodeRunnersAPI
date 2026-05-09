using Microsoft.AspNetCore.Identity;

namespace SGA.Identity
{
    public class AppIdentityUser : IdentityUser
    {
        public int? UsuarioId { get; set; } 
    }
}
