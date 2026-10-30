using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaInventario.API.Data;
using SistemaInventario.API.DTOs.Paginacion;
using SistemaInventario.API.DTOs.Usuario;
using SistemaInventario.API.Models;
using SistemaInventario.API.Services;
using System.Security.Claims;

namespace SistemaInventario.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = Roles.Administrador)]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuditoriaService _auditoria;

        public UsuariosController(AppDbContext context, IAuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
        }

        private int UsuarioActualId =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        // GET: api/usuarios?buscar=juan
        [HttpGet]
        public async Task<ActionResult<RespuestaPaginadaDto<UsuarioDto>>> GetUsuarios(
            int page = 1, int pageSize = 10, string? buscar = null)
        {
            if (page < 1)
                return BadRequest(new { mensaje = "El número de página debe ser mayor o igual a 1." });

            if (pageSize < 1 || pageSize > 50)
                return BadRequest(new { mensaje = "El tamaño de página debe estar entre 1 y 50." });

            var query = _context.Usuarios.AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var t = buscar.Trim();
                query = query.Where(u => u.Nombre.Contains(t) || u.Email.Contains(t));
            }

            var total = await query.CountAsync();

            var datos = await query
                .OrderBy(u => u.Nombre)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UsuarioDto
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Email = u.Email,
                    Rol = u.Rol
                })
                .ToListAsync();

            return Ok(new RespuestaPaginadaDto<UsuarioDto>
            {
                Pagina = page,
                TamanoPagina = pageSize,
                TotalRegistros = total,
                TotalPaginas = (int)Math.Ceiling(total / (double)pageSize),
                Datos = datos
            });
        }

        // PUT: api/usuarios/5/rol
        [HttpPut("{id}/rol")]
        public async Task<IActionResult> CambiarRol(int id, CambiarRolDto dto)
        {
            if (!Roles.Todos.Contains(dto.Rol))
            {
                return BadRequest(new
                {
                    mensaje = $"Rol inválido. Valores permitidos: {string.Join(", ", Roles.Todos)}."
                });
            }

            if (id == UsuarioActualId)
            {
                return BadRequest(new { mensaje = "No podés cambiar tu propio rol." });
            }

            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                return NotFound(new { mensaje = "El usuario no existe." });

            var anterior = usuario.Rol;
            usuario.Rol = dto.Rol;
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync("CAMBIAR_ROL", "Usuario", usuario.Id, UsuarioActualId,
                $"{anterior} -> {dto.Rol}");

            return NoContent();
        }
    }
}