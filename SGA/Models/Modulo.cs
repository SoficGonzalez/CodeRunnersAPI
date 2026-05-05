using System.ComponentModel.DataAnnotations;

namespace SGA.Models;

public class Modulo
{
    public int ModuloId { get; set; }

    [Required, MaxLength(40)]
    public string CodigoModulo { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string NombreModulo { get; set; } = string.Empty;

    public int Orden { get; set; }

    public ICollection<RolModulo> RolModulos { get; set; } = new List<RolModulo>();
}
