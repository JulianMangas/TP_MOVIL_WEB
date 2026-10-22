using System.ComponentModel.DataAnnotations;

namespace SistemaInventario.API.Models
{
    public class Auditoria
    {
        [Key]
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public int? UsuarioId { get; set; }

        public Usuario? Usuario { get; set; }

        [Required]
        [MaxLength(50)]
        public string Accion { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Entidad { get; set; } = string.Empty;

        public int? EntidadId { get; set; }

        [MaxLength(500)]
        public string? Detalle { get; set; }
    }
}