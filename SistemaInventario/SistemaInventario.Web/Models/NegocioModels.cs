namespace SistemaInventario.Web.Models
{
    public class RespuestaPaginada<T>
    {
        public int Pagina { get; set; }
        public int TamanoPagina { get; set; }
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
        public List<T> Datos { get; set; } = new();
    }

    public class ProductoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string? ImagenUrl { get; set; }
        public int CategoriaId { get; set; }
        public string? CategoriaNombre { get; set; }
    }

    public class ClienteDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Direccion { get; set; }
    }

    public class CrearVentaRequest
    {
        public int? ClienteId { get; set; }
        public List<DetalleVentaRequest> Detalles { get; set; } = new();
    }

    public class DetalleVentaRequest
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
    }

    public class VentaRespuesta
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public int ClienteId { get; set; }
        public string? ClienteNombre { get; set; }
        public int UsuarioId { get; set; }
        public string? CajeroNombre { get; set; }
        public decimal Total { get; set; }
        public List<DetalleVentaRespuesta> Detalles { get; set; } = new();
    }

    public class DetalleVentaRespuesta
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string? ProductoNombre { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioVenta { get; set; }
    }

    public class ProveedorDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Direccion { get; set; }
    }

    public class CrearIngresoRequest
    {
        public int ProveedorId { get; set; }
        public List<DetalleIngresoRequest> Detalles { get; set; } = new();
    }

    public class DetalleIngresoRequest
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioCompra { get; set; }
    }

    public class IngresoRespuesta
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public int ProveedorId { get; set; }
        public string? ProveedorNombre { get; set; }
        public List<DetalleIngresoRespuesta> Detalles { get; set; } = new();
    }

    public class DetalleIngresoRespuesta
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string? ProductoNombre { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioCompra { get; set; }
    }
    public class UsuarioDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }

    public class CambiarRolRequest
    {
        public string Rol { get; set; } = string.Empty;
    }
}