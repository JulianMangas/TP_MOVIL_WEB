using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaInventario.API.DTOs.Ingreso;
using SistemaInventario.API.DTOs.Paginacion;
using SistemaInventario.API.Services;
using System.Security.Claims;

namespace SistemaInventario.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador")]
    public class IngresosController : ControllerBase
    {
        private readonly IIngresoService _ingresoService;

        public IngresosController(IIngresoService ingresoService)
        {
            _ingresoService = ingresoService;
        }

        private int UsuarioActualId =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        // GET: api/ingresos
        [HttpGet]
        public async Task<ActionResult<RespuestaPaginadaDto<IngresoDto>>> GetIngresos(
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

            var respuesta = await _ingresoService.ListarAsync(page, pageSize);

            return Ok(respuesta);
        }

        // GET: api/ingresos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IngresoDto>> GetIngreso(int id)
        {
            var ingreso = await _ingresoService.ObtenerAsync(id);

            if (ingreso == null)
            {
                return NotFound(new
                {
                    mensaje = "El ingreso no existe."
                });
            }

            return Ok(ingreso);
        }

        // POST: api/ingresos
        [HttpPost]
        public async Task<ActionResult<IngresoDto>> CrearIngreso(CrearIngresoDto dto)
        {
            var ingreso = await _ingresoService.CrearAsync(dto, UsuarioActualId);

            return CreatedAtAction(nameof(GetIngreso), new { id = ingreso.Id }, ingreso);
        }
    }
}