using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaInventario.API.Data;
using SistemaInventario.API.DTOs.Cliente;
using SistemaInventario.API.DTOs.Paginacion;
using SistemaInventario.API.Models;

namespace SistemaInventario.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = Roles.CajeroOAdmin)]
    public class ClientesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClientesController(AppDbContext context)
        {
            _context = context;
        }

        private static readonly Expression<Func<Cliente, ClienteDto>> ADto = c => new ClienteDto
        {
            Id = c.Id,
            Nombre = c.Nombre,
            Apellido = c.Apellido,
            Telefono = c.Telefono,
            Email = c.Email,
            Direccion = c.Direccion
        };

        private static bool EsConsumidorFinal(Cliente c) =>
            c.UsuarioId == null &&
            c.Nombre == ConsumidorFinal.Nombre &&
            c.Apellido == ConsumidorFinal.Apellido;

        // GET: api/clientes?buscar=perez
        [HttpGet]
        public async Task<ActionResult<RespuestaPaginadaDto<ClienteDto>>> GetClientes(
            int page = 1, int pageSize = 10, string? buscar = null)
        {
            if (page < 1)
                return BadRequest(new { mensaje = "El número de página debe ser mayor o igual a 1." });

            if (pageSize < 1 || pageSize > 50)
                return BadRequest(new { mensaje = "El tamaño de página debe estar entre 1 y 50." });

            var query = _context.Clientes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var t = buscar.Trim();
                query = query.Where(c =>
                    c.Nombre.Contains(t) ||
                    c.Apellido.Contains(t) ||
                    (c.Email != null && c.Email.Contains(t)));
            }

            var total = await query.CountAsync();

            var datos = await query
                .OrderBy(c => c.Apellido).ThenBy(c => c.Nombre)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ADto)
                .ToListAsync();

            return Ok(new RespuestaPaginadaDto<ClienteDto>
            {
                Pagina = page,
                TamanoPagina = pageSize,
                TotalRegistros = total,
                TotalPaginas = (int)Math.Ceiling(total / (double)pageSize),
                Datos = datos
            });
        }

        // GET: api/clientes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteDto>> GetCliente(int id)
        {
            var cliente = await _context.Clientes
                .Where(c => c.Id == id)
                .Select(ADto)
                .FirstOrDefaultAsync();

            if (cliente == null)
                return NotFound(new { mensaje = "El cliente no existe." });

            return Ok(cliente);
        }

        // POST: api/clientes  (Cajero y Administrador)
        [HttpPost]
        public async Task<ActionResult<ClienteDto>> CrearCliente(CrearClienteDto dto)
        {
            var cliente = new Cliente
            {
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Telefono = dto.Telefono,
                Email = dto.Email,
                Direccion = dto.Direccion
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            var resultado = await _context.Clientes
                .Where(c => c.Id == cliente.Id)
                .Select(ADto)
                .FirstAsync();

            return CreatedAtAction(nameof(GetCliente), new { id = cliente.Id }, resultado);
        }

        // PUT: api/clientes/5  (solo Administrador)
        [HttpPut("{id}")]
        [Authorize(Roles = Roles.Administrador)]
        public async Task<IActionResult> ActualizarCliente(int id, ActualizarClienteDto dto)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
                return NotFound(new { mensaje = "El cliente no existe." });

            if (EsConsumidorFinal(cliente))
                return BadRequest(new { mensaje = "El cliente 'Consumidor Final' no se puede modificar." });

            cliente.Nombre = dto.Nombre;
            cliente.Apellido = dto.Apellido;
            cliente.Telefono = dto.Telefono;
            cliente.Email = dto.Email;
            cliente.Direccion = dto.Direccion;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/clientes/5  (solo Administrador)
        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Administrador)]
        public async Task<IActionResult> EliminarCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
                return NotFound(new { mensaje = "El cliente no existe." });

            if (EsConsumidorFinal(cliente))
                return BadRequest(new { mensaje = "El cliente 'Consumidor Final' no se puede eliminar." });

            if (cliente.UsuarioId != null)
                return BadRequest(new { mensaje = "No se puede eliminar un cliente asociado a un usuario." });

            if (await _context.Ventas.AnyAsync(v => v.ClienteId == id))
                return BadRequest(new { mensaje = "No se puede eliminar el cliente porque tiene ventas asociadas." });

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}