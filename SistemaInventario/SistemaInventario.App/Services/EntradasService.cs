using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaInventario.App.Models;

namespace SistemaInventario.App.Services;

public class EntradasService
{
    private readonly ApiClient _api;

    public EntradasService(ApiClient api)
    {
        _api = api;
    }

    public async Task<(List<ProveedorDto>? Datos, string? Error)> ObtenerProveedoresAsync()
    {
        var (respuesta, error) = await _api.GetAsync<RespuestaPaginadaDto<ProveedorDto>>(
            "api/Proveedores?page=1&pageSize=50");

        return (respuesta?.Datos, error);
    }

    public Task<(IngresoDto? Datos, string? Error)> RegistrarAsync(CrearIngresoDto dto)
        => _api.PostAsync<CrearIngresoDto, IngresoDto>("api/Ingresos", dto);
}