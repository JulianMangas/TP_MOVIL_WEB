using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SistemaInventario.API.Data;
using SistemaInventario.API.DTOs.Ingreso;
using SistemaInventario.API.DTOs.Paginacion;
using SistemaInventario.API.Exceptions;
using SistemaInventario.API.Models;

namespace SistemaInventario.API.Services
{
    public class IngresoService : IIngresoService
    {
        private readonly AppDbContext _context;
        private readonly IAuditoriaService _auditoria;

        public IngresoService(AppDbContext context, IAuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
        }

        private static readonly Expression<Func<Ingreso, IngresoDto>> AIngresoDto = i => new IngresoDto
        {
            Id = i.Id,
            Fecha = i.Fecha,
            ProveedorId = i.ProveedorId,
            ProveedorNombre = i.Proveedor != null
                ? i.Proveedor.Nombre
                : null,

            Detalles = i.Detalles.Select(d => new DetalleIngresoDto
            {
                Id = d.Id,
                ProductoId = d.ProductoId,
                ProductoNombre = d.Producto != null
                    ? d.Producto.Nombre
                    : null,
                Cantidad = d.Cantidad,
                PrecioCompra = d.PrecioCompra
            }).ToList()
        };

        public async Task<RespuestaPaginadaDto<IngresoDto>> ListarAsync(int page, int pageSize)
        {
            var query = _context.Ingresos.AsQueryable();

            var totalRegistros = await query.CountAsync();

            var ingresos = await query
                .OrderBy(i => i.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(AIngresoDto)
                .ToListAsync();

            return new RespuestaPaginadaDto<IngresoDto>
            {
                Pagina = page,
                TamanoPagina = pageSize,
                TotalRegistros = totalRegistros,
                TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)pageSize),
                Datos = ingresos
            };
        }

        public async Task<IngresoDto?> ObtenerAsync(int id)
        {
            return await _context.Ingresos
                .Where(i => i.Id == id)
                .Select(AIngresoDto)
                .FirstOrDefaultAsync();
        }

        public async Task<IngresoDto> CrearAsync(CrearIngresoDto dto, int usuarioId)
        {
            var proveedorExiste = await _context.Proveedores
                .AnyAsync(p => p.Id == dto.ProveedorId);

            if (!proveedorExiste)
            {
                throw new ReglaDeNegocioException("El proveedor indicado no existe.");
            }

            var ingreso = new Ingreso
            {
                ProveedorId = dto.ProveedorId,
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

                ingreso.Detalles.Add(new DetalleIngreso
                {
                    ProductoId = producto.Id,
                    Cantidad = detalleDto.Cantidad,
                    PrecioCompra = detalleDto.PrecioCompra
                });

                producto.Stock += detalleDto.Cantidad;
            }

            _context.Ingresos.Add(ingreso);

            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync("CREAR", "Ingreso", ingreso.Id, usuarioId,
                $"Ingreso de {ingreso.Detalles.Count} producto(s)");

            return await _context.Ingresos
                .Where(i => i.Id == ingreso.Id)
                .Select(AIngresoDto)
                .FirstAsync();
        }
    }
}