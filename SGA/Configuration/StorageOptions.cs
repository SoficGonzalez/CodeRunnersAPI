namespace SGA.Configuration;

public class StorageOptions
{
    public const string SectionName = "Storage";

    public string RutaPlantillas { get; set; } = "C:\\SGA\\Repositorio\\Plantillas";

    public int TamanoMaximoMB { get; set; } = 50;

    public long TamanoMaximoBytes => (long)TamanoMaximoMB * 1024L * 1024L;
}
