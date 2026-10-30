using System.ComponentModel.DataAnnotations;

namespace SistemaInventario.API.DTOs.Usuario
{
    public class UsuarioDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }

    public class CambiarRolDto
    {
        [Required]
        public string Rol { get; set; } = string.Empty;
    }
}