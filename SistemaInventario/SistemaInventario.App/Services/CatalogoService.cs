using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaInventario.App.Models;

namespace SistemaInventario.App.Services;

public class CatalogoService
{
    private readonly ApiClient _api;
    private readonly HttpClient _http;

    public CatalogoService(ApiClient api, HttpClient http)
    {
        _api = api;
        _http = http;
    }

    public Task<(RespuestaPaginadaDto<ProductoDto>? Datos, string? Error)> ObtenerProductosAsync(
        int pagina, int tamano, string? buscar, int? categoriaId)
    {
        var url = $"api/Productos?page={pagina}&pageSize={tamano}";

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            url += $"&buscar={Uri.EscapeDataString(buscar.Trim())}";
        }

        if (categoriaId.HasValue)
        {
            url += $"&categoriaId={categoriaId.Value}";
        }

        return _api.GetAsync<RespuestaPaginadaDto<ProductoDto>>(url);
    }

    public async Task<(List<CategoriaDto>? Datos, string? Error)> ObtenerCategoriasAsync()
    {
        var (respuesta, error) = await _api.GetAsync<RespuestaPaginadaDto<CategoriaDto>>(
            "api/Categorias?page=1&pageSize=50");

        return (respuesta?.Datos, error);
    }

    // La API devuelve rutas relativas (/uploads/productos/xxx.png)
    public string? UrlImagen(string? ruta)
    {
        if (string.IsNullOrWhiteSpace(ruta))
        {
            return null;
        }

        return new Uri(_http.BaseAddress!, ruta).ToString();
    }
}
