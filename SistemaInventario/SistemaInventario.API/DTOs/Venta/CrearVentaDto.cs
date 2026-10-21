using System.ComponentModel.DataAnnotations;

namespace SistemaInventario.API.DTOs.Venta
{
    public class CrearVentaDto
    {
        [Required]
        [MinLength(1)]
        public List<CrearDetalleVentaDto> Detalles { get; set; } = new();
    }

    public class CrearDetalleVentaDto
    {
        [Range(1, int.MaxValue)]
        public int ProductoId { get; set; }

        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }
    }
}