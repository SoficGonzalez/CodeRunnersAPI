namespace SGA.DTOs;

public class PlantillaDetalleResponse
{
    public int PlantillaId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public int EstadoPlantillaId { get; set; }

    public string? EstadoActual { get; set; }

    public int CreadoPorUsuarioId { get; set; }

    public string? StoragePath { get; set; }

    public bool Activa { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime FechaActualizacion { get; set; }

    public List<CampoPlantillaDetalleResponse> Campos { get; set; } = new();
}

public class CampoPlantillaDetalleResponse
{
    public int CampoPlantillaId { get; set; }

    public string ClaveCampo { get; set; } = string.Empty;

    public string EtiquetaPantalla { get; set; } = string.Empty;

    public int Orden { get; set; }

    public bool Obligatorio { get; set; }

    public string TipoDato { get; set; } = string.Empty;

    public string? ValorPorDefecto { get; set; }
}