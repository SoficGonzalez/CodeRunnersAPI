using System.ComponentModel.DataAnnotations;

namespace SGA.Models;

public class Usuario
{
    public int UsuarioId { get; set; }

    public int RolId { get; set; }
    public Rol Rol { get; set; } = null!;

    [Required, MaxLength(60)]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string NombreCompleto { get; set; } = string.Empty;
    public int? SedeId { get; set; }
    public Sede? Sede { get; set; }

    [Required, MaxLength(150), EmailAddress]
    public string Correo { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public ICollection<Plantilla> PlantillasCreadas { get; set; } = new List<Plantilla>();
    public ICollection<DocumentoLlenado> DocumentosLlenados { get; set; } = new List<DocumentoLlenado>();
    public ICollection<ArchivoEvidencia> EvidenciasSubidas { get; set; } = new List<ArchivoEvidencia>();
}
