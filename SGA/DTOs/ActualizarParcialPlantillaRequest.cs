using System.ComponentModel.DataAnnotations;

namespace SGA.DTOs
{
    public class ActualizarParcialPlantillaRequest
    {
        [MaxLength(180)]
        public string? Nombre { get; set; }

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "EstadoPlantillaId debe ser mayor a 0.")]
        public int? EstadoPlantillaId { get; set; }

        public bool? Activa { get; set; }
    }
}