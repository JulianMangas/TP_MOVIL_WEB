using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaInventario.API.Data;
using SistemaInventario.API.DTOs.Auditoria;
using SistemaInventario.API.DTOs.Paginacion;

namespace SistemaInventario.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador")]
    public class AuditoriaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuditoriaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/auditoria?entidad=Venta
        [HttpGet]
        public async Task<ActionResult<RespuestaPaginadaDto<AuditoriaDto>>> GetAuditoria(
            int page = 1,
            int pageSize = 10,
            string? entidad = null)
        {
            if (page < 1)
            {
                return BadRequest(new
                {
                    mensaje = "El número de página debe ser mayor o igual a 1."
                });
            }

            if (pageSize < 1 || pageSize > 50)
            {
                return BadRequest(new
                {
                    mensaje = "El tamaño de página debe estar entre 1 y 50."
                });
            }

            var query = _context.Auditorias.AsQueryable();

            if (!string.IsNullOrWhiteSpace(entidad))
            {
                query = query.Where(a => a.Entidad == entidad);
            }

            var totalRegistros = await query.CountAsync();

            var datos = await query
                .OrderByDescending(a => a.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AuditoriaDto
                {
                    Id = a.Id,
                    Fecha = a.Fecha,
                    UsuarioId = a.UsuarioId,
                    UsuarioNombre = a.Usuario != null ? a.Usuario.Nombre : null,
                    Accion = a.Accion,
                    Entidad = a.Entidad,
                    EntidadId = a.EntidadId,
                    Detalle = a.Detalle
                })
                .ToListAsync();

            return Ok(new RespuestaPaginadaDto<AuditoriaDto>
            {
                Pagina = page,
                TamanoPagina = pageSize,
                TotalRegistros = totalRegistros,
                TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)pageSize),
                Datos = datos
            });
        }

        // GET: api/auditoria/errores
        [HttpGet("errores")]
        public async Task<ActionResult<RespuestaPaginadaDto<LogErrorDto>>> GetErrores(
            int page = 1,
            int pageSize = 10)
        {
            if (page < 1)
            {
                return BadRequest(new
                {
                    mensaje = "El número de página debe ser mayor o igual a 1."
                });
            }

            if (pageSize < 1 || pageSize > 50)
            {
                return BadRequest(new
                {
                    mensaje = "El tamaño de página debe estar entre 1 y 50."
                });
            }

            var query = _context.LogsError.AsQueryable();

            var totalRegistros = await query.CountAsync();

            var datos = await query
                .OrderByDescending(e => e.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new LogErrorDto
                {
                    Id = e.Id,
                    Fecha = e.Fecha,
                    UsuarioId = e.UsuarioId,
                    Metodo = e.Metodo,
                    Ruta = e.Ruta,
                    Mensaje = e.Mensaje,
                    Detalle = e.Detalle
                })
                .ToListAsync();

            return Ok(new RespuestaPaginadaDto<LogErrorDto>
            {
                Pagina = page,
                TamanoPagina = pageSize,
                TotalRegistros = totalRegistros,
                TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)pageSize),
                Datos = datos
            });
        }
    }
}
