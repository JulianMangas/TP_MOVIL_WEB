using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaInventario.API.DTOs.Paginacion;
using SistemaInventario.API.DTOs.Venta;
using SistemaInventario.API.Services;
using System.Security.Claims;
using SistemaInventario.API.Models;

namespace SistemaInventario.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = Roles.CajeroOAdmin)]
    public class VentasController : ControllerBase
    {
        private readonly IVentaService _ventaService;

        public VentasController(IVentaService ventaService)
        {
            _ventaService = ventaService;
        }

        private int UsuarioActualId =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        private bool EsAdmin => User.IsInRole("Administrador");

        // GET: api/ventas
        [HttpGet]
        public async Task<ActionResult<RespuestaPaginadaDto<VentaDto>>> GetVentas(
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

            var respuesta = await _ventaService.ListarAsync(
                page, pageSize, UsuarioActualId, EsAdmin);

            return Ok(respuesta);
        }

        // GET: api/ventas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VentaDto>> GetVenta(int id)
        {
            var venta = await _ventaService.ObtenerAsync(id, UsuarioActualId, EsAdmin);

            if (venta == null)
            {
                return NotFound(new
                {
                    mensaje = "La venta no existe."
                });
            }

            return Ok(venta);
        }

        // POST: api/ventas
        [HttpPost]
        public async Task<ActionResult<VentaDto>> CrearVenta(CrearVentaDto dto)
        {
            var venta = await _ventaService.CrearAsync(dto, UsuarioActualId);

            return CreatedAtAction(nameof(GetVenta), new { id = venta.Id }, venta);
        }
    }
}