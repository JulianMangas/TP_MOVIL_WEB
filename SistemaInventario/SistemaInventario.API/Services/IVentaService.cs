using SistemaInventario.API.DTOs.Paginacion;
using SistemaInventario.API.DTOs.Venta;

namespace SistemaInventario.API.Services
{
    public interface IVentaService
    {
        Task<RespuestaPaginadaDto<VentaDto>> ListarAsync(
            int page, int pageSize, int usuarioId, bool esAdmin);

        Task<VentaDto?> ObtenerAsync(int id, int usuarioId, bool esAdmin);

        Task<VentaDto> CrearAsync(CrearVentaDto dto, int usuarioId);
    }
}