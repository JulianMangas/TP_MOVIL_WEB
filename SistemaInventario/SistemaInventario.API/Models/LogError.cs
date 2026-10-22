using System.ComponentModel.DataAnnotations;

namespace SistemaInventario.API.Models
{
    public class LogError
    {
        [Key]
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public int? UsuarioId { get; set; }

        [Required]
        [MaxLength(10)]
        public string Metodo { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public string Ruta { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Mensaje { get; set; } = string.Empty;

        [Required]
        public string Detalle { get; set; } = string.Empty;
    }
}