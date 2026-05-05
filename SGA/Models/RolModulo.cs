namespace SGA.Models;

public class RolModulo
{
    public int RolModuloId { get; set; }

    public int RolId { get; set; }
    public Rol Rol { get; set; } = null!;

    public int ModuloId { get; set; }
    public Modulo Modulo { get; set; } = null!;
}
