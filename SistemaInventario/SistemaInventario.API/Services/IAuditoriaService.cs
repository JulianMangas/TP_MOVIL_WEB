namespace SistemaInventario.API.Services
{
    public interface IAuditoriaService
    {
        Task RegistrarAsync(
            string accion,
            string entidad,
            int? entidadId,
            int? usuarioId,
            string? detalle = null);
    }
}