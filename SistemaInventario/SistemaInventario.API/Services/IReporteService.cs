namespace SistemaInventario.API.Services
{
    public interface IReporteService
    {
        Task<(byte[] Contenido, string NombreArchivo)> GenerarDiarioAsync(DateTime? fecha);
        Task<(byte[] Contenido, string NombreArchivo)> GenerarAuditoriaAsync();
        Task<(byte[] Contenido, string NombreArchivo)> GenerarErroresAsync();
    }
}