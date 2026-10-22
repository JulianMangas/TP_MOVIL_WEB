namespace SistemaInventario.App.Models;

public class ProveedorDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
}

public class CrearIngresoDto
{
    public int ProveedorId { get; set; }
    public List<CrearDetalleIngresoDto> Detalles { get; set; } = new();
}

public class CrearDetalleIngresoDto
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioCompra { get; set; }
}

public class IngresoDto
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public int ProveedorId { get; set; }
    public string? ProveedorNombre { get; set; }
}

// Una línea de la lista que el administrador va armando en pantalla
public class LineaIngreso
{
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = "";
    public int Cantidad { get; set; }
    public decimal PrecioCompra { get; set; }

    public decimal Subtotal => Cantidad * PrecioCompra;
}