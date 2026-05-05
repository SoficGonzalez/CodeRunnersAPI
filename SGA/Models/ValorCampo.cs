namespace SGA.Models;

public class ValorCampo
{
    public int ValorCampoId { get; set; }

    public int DocumentoLlenadoId { get; set; }
    public DocumentoLlenado DocumentoLlenado { get; set; } = null!;

    public int CampoPlantillaId { get; set; }
    public CampoPlantilla CampoPlantilla { get; set; } = null!;

    public string? TextoValor { get; set; }

    public DateTime FechaGuardado { get; set; } = DateTime.UtcNow;
}
