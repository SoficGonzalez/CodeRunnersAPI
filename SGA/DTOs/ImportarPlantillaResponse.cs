namespace SGA.DTOs;

public class ImportarPlantillaResponse
{
    public int PlantillaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int EstadoPlantillaId { get; set; }
    public int CreadoPorUsuarioId { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public List<CampoDetectadoDto> CamposDetectados { get; set; } = new();
    public DateTime FechaCreacion { get; set; }
}
