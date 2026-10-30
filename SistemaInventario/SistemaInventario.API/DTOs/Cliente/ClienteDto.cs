using System.ComponentModel.DataAnnotations;

namespace SistemaInventario.API.DTOs.Cliente
{
    public class ClienteDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Direccion { get; set; }
    }

    public class CrearClienteDto
    {
        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Apellido { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Telefono { get; set; }

        [MaxLength(150), EmailAddress]
        public string? Email { get; set; }

        [MaxLength(250)]
        public string? Direccion { get; set; }
    }

    public class ActualizarClienteDto : CrearClienteDto
    {
    }
}