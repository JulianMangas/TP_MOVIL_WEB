using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SistemaInventario.API.Data;
using SistemaInventario.API.DTOs.Auth;
using SistemaInventario.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using SistemaInventario.API.Services;
using System.Text;

namespace SistemaInventario.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IAuditoriaService _auditoria;

        public AuthController(
            AppDbContext context,
            IConfiguration configuration,
            IAuditoriaService auditoria)
        {
            _context = context;
            _configuration = configuration;
            _auditoria = auditoria;
        }

        [HttpPost("registro")]
        public async Task<IActionResult> Registro(RegistroDto dto)
        {
            var emailExiste = await _context.Usuarios
                .AnyAsync(u => u.Email == dto.Email);

            if (emailExiste)
            {
                return BadRequest(new
                {
                    mensaje = "Ya existe un usuario con ese email."
                });
            }

            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Rol = "Usuario",
                Cliente = new Cliente
                {
                    Nombre = dto.Nombre,
                    Apellido = dto.Apellido,
                    Email = dto.Email
                }
            };

            _context.Usuarios.Add(usuario);

            await _context.SaveChangesAsync();


            return Ok(new
            {
                mensaje = "Usuario registrado correctamente.",
                usuarioId = usuario.Id,
                nombre = usuario.Nombre,
                email = usuario.Email,
                rol = usuario.Rol
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginRespuestaDto>> Login(LoginDto dto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (usuario == null)
            {
                await _auditoria.RegistrarAsync("LOGIN_FALLIDO", "Usuario", null, null,
                    $"Email inexistente: {dto.Email}");

                return Unauthorized(new
                {
                    mensaje = "Email o contraseña incorrectos."
                });
            }

            bool passwordCorrecta;

            try
            {
                passwordCorrecta = BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                passwordCorrecta = false;
            }

            if (!passwordCorrecta)
            {
                await _auditoria.RegistrarAsync("LOGIN_FALLIDO", "Usuario", usuario.Id, null,
                    "Contraseña incorrecta");

                return Unauthorized(new
                {
                    mensaje = "Email o contraseña incorrectos."
                });
            }

            await _auditoria.RegistrarAsync("LOGIN", "Usuario", usuario.Id, usuario.Id);

            var token = GenerarToken(usuario);

            return Ok(new LoginRespuestaDto
            {
                Token = token,
                UsuarioId = usuario.Id,
                Nombre = usuario.Nombre,
                Rol = usuario.Rol
            });
        }

        private string GenerarToken(Usuario usuario)
        {
            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuario.Id.ToString()
                ),

                new Claim(
                    ClaimTypes.Name,
                    usuario.Nombre
                ),

                new Claim(
                    ClaimTypes.Email,
                    usuario.Email
                ),

                new Claim(
                    ClaimTypes.Role,
                    usuario.Rol
                )
            };

            var clave = _configuration["Jwt:Key"];

            if (string.IsNullOrEmpty(clave))
            {
                throw new InvalidOperationException(
                    "No se configuró la clave JWT."
                );
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(clave)
            );

            var credenciales = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(3),
                signingCredentials: credenciales
            );

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}