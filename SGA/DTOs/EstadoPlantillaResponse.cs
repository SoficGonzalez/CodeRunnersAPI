namespace SGA.DTOs;

public class EstadoPlantillaResponse
{
    public int EstadoPlantillaId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int Orden { get; set; }
    public bool Activo { get; set; }
}

