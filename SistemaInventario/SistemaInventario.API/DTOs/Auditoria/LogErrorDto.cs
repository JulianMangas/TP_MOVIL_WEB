namespace SistemaInventario.API.DTOs.Auditoria
{
    public class LogErrorDto
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public int? UsuarioId { get; set; }

        public string Metodo { get; set; } = string.Empty;

        public string Ruta { get; set; } = string.Empty;

        public string Mensaje { get; set; } = string.Empty;

        public string Detalle { get; set; } = string.Empty;
    }
}