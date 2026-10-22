using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SistemaInventario.API.Data;
using SistemaInventario.API.DTOs.Paginacion;
using SistemaInventario.API.DTOs.Venta;
using SistemaInventario.API.Exceptions;
using SistemaInventario.API.Models;

namespace SistemaInventario.API.Services
{
    public class VentaService : IVentaService
    {
        private readonly AppDbContext _context;
        private readonly IAuditoriaService _auditoria;

        public VentaService(AppDbContext context, IAuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
        }

        // La proyección a DTO
        private static readonly Expression<Func<Venta, VentaDto>> AVentaDto = v => new VentaDto
        {
            Id = v.Id,
            Fecha = v.Fecha,
            ClienteId = v.ClienteId,
            ClienteNombre = v.Cliente != null
                ? v.Cliente.Nombre + " " + v.Cliente.Apellido
                : null,

            Detalles = v.Detalles.Select(d => new DetalleVentaDto
            {
                Id = d.Id,
                ProductoId = d.ProductoId,
                ProductoNombre = d.Producto != null
                    ? d.Producto.Nombre
                    : null,
                Cantidad = d.Cantidad,
                PrecioVenta = d.PrecioVenta
            }).ToList()
        };

        public async Task<RespuestaPaginadaDto<VentaDto>> ListarAsync(
            int page, int pageSize, int usuarioId, bool esAdmin)
        {
            var query = _context.Ventas.AsQueryable();

            if (!esAdmin)
            {
                query = query.Where(v => v.UsuarioId == usuarioId);
            }

            var totalRegistros = await query.CountAsync();

            var ventas = await query
                .OrderBy(v => v.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(AVentaDto)
                .ToListAsync();

            return new RespuestaPaginadaDto<VentaDto>
            {
                Pagina = page,
                TamanoPagina = pageSize,
                TotalRegistros = totalRegistros,
                TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)pageSize),
                Datos = ventas
            };
        }

        public async Task<VentaDto?> ObtenerAsync(int id, int usuarioId, bool esAdmin)
        {
            return await _context.Ventas
                .Where(v => v.Id == id && (esAdmin || v.UsuarioId == usuarioId))
                .Select(AVentaDto)
                .FirstOrDefaultAsync();
        }

        public async Task<VentaDto> CrearAsync(CrearVentaDto dto, int usuarioId)
        {
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

            if (cliente == null)
            {
                throw new ReglaDeNegocioException(
                    "Tu usuario no tiene un cliente asociado, no puede realizar compras.");
            }

            var venta = new Venta
            {
                ClienteId = cliente.Id,
                UsuarioId = usuarioId,
                Fecha = DateTime.Now
            };

            foreach (var detalleDto in dto.Detalles)
            {
                var producto = await _context.Productos
                    .FirstOrDefaultAsync(p => p.Id == detalleDto.ProductoId);

                if (producto == null)
                {
                    throw new ReglaDeNegocioException(
                        $"El producto con ID {detalleDto.ProductoId} no existe.");
                }

                if (producto.Stock < detalleDto.Cantidad)
                {
                    throw new ReglaDeNegocioException(
                        $"Stock insuficiente para el producto '{producto.Nombre}'. " +
                        $"Stock disponible: {producto.Stock}. " +
                        $"Cantidad solicitada: {detalleDto.Cantidad}.");
                }

                venta.Detalles.Add(new DetalleVenta
                {
                    ProductoId = producto.Id,
                    Cantidad = detalleDto.Cantidad,
                    PrecioVenta = producto.Precio
                });

                producto.Stock -= detalleDto.Cantidad;
            }

            _context.Ventas.Add(venta);

            await _context.SaveChangesAsync();
            await _auditoria.RegistrarAsync("CREAR", "Venta", venta.Id, usuarioId,
            $"Venta de {venta.Detalles.Count} producto(s)");
            return await _context.Ventas
                .Where(v => v.Id == venta.Id)
                .Select(AVentaDto)
                .FirstAsync();
        }
    }
}