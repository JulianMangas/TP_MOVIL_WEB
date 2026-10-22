using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaInventario.App.Models;

public class CrearVentaDto
{
    public List<CrearDetalleVentaDto> Detalles { get; set; } = new();
}

public class CrearDetalleVentaDto
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
}

public class VentaDto
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public int ClienteId { get; set; }
    public string? ClienteNombre { get; set; }
    public List<DetalleVentaDto> Detalles { get; set; } = new();

    public decimal Total => Detalles.Sum(d => d.Cantidad * d.PrecioVenta);
}

public class DetalleVentaDto
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string? ProductoNombre { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioVenta { get; set; }
}

public class ItemCarrito
{
    public int ProductoId { get; set; }
    public string Nombre { get; set; } = "";
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public string? ImagenUrl { get; set; }
    public int Cantidad { get; set; }

    public decimal Subtotal => Precio * Cantidad;
}