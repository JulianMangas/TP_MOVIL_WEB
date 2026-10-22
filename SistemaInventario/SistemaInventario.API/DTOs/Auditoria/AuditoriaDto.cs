namespace SistemaInventario.API.DTOs.Auditoria
{
    public class AuditoriaDto
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public int? UsuarioId { get; set; }

        public string? UsuarioNombre { get; set; }

        public string Accion { get; set; } = string.Empty;

        public string Entidad { get; set; } = string.Empty;

        public int? EntidadId { get; set; }

        public string? Detalle { get; set; }
    }
}