using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaInventario.API.Services;
using System.Security.Claims;

namespace SistemaInventario.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador")]
    public class ReportesController : ControllerBase
    {
        private readonly IReporteService _reportes;
        private readonly IAuditoriaService _auditoria;

        public ReportesController(IReporteService reportes, IAuditoriaService auditoria)
        {
            _reportes = reportes;
            _auditoria = auditoria;
        }

        private int UsuarioActualId =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        // GET: api/reportes/diario            (hoy)
        // GET: api/reportes/diario?fecha=2026-10-22
        [HttpGet("diario")]
        public async Task<IActionResult> Diario(DateTime? fecha = null)
        {
            var (contenido, nombre) = await _reportes.GenerarDiarioAsync(fecha);
            await Registrar("diario");
            return File(contenido, "application/pdf", nombre);
        }

        // GET: api/reportes/auditoria
        [HttpGet("auditoria")]
        public async Task<IActionResult> Auditoria()
        {
            var (contenido, nombre) = await _reportes.GenerarAuditoriaAsync();
            await Registrar("auditoria");
            return File(contenido, "application/pdf", nombre);
        }

        // GET: api/reportes/errores
        [HttpGet("errores")]
        public async Task<IActionResult> Errores()
        {
            var (contenido, nombre) = await _reportes.GenerarErroresAsync();
            await Registrar("errores");
            return File(contenido, "application/pdf", nombre);
        }

        private Task Registrar(string tipo) =>
            _auditoria.RegistrarAsync("DESCARGAR", "Reporte", null, UsuarioActualId, tipo);
    }
}