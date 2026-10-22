using SistemaInventario.API.DTOs.Ingreso;
using SistemaInventario.API.DTOs.Paginacion;

namespace SistemaInventario.API.Services
{
    public interface IIngresoService
    {
        Task<RespuestaPaginadaDto<IngresoDto>> ListarAsync(int page, int pageSize);

        Task<IngresoDto?> ObtenerAsync(int id);

        Task<IngresoDto> CrearAsync(CrearIngresoDto dto, int usuarioId);
    }
}