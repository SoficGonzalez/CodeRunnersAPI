using System.ComponentModel.DataAnnotations;

namespace SGA.Models;

public class Rol
{
    public int RolId { get; set; }

    [Required, MaxLength(60)]
    public string NombreRol { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    public ICollection<RolModulo> RolModulos { get; set; } = new List<RolModulo>();
}
