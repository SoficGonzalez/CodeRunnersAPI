using SGA.Identity;

namespace SGA.Services
{
    public interface IJwtService
    {
        string GenerarToken(AppIdentityUser user, IList<string> roles);
    }
}
