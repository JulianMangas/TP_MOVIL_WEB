using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaInventario.App.Models;

namespace SistemaInventario.App.Services;

public class CompraService
{
    private readonly ApiClient _api;
    private readonly CarritoService _carrito;

    public CompraService(ApiClient api, CarritoService carrito)
    {
        _api = api;
        _carrito = carrito;
    }

    public async Task<(VentaDto? Venta, string? Error)> ConfirmarAsync()
    {
        if (_carrito.Items.Count == 0)
        {
            return (null, "El carrito está vacío.");
        }

        var dto = new CrearVentaDto
        {
            Detalles = _carrito.Items
                .Select(i => new CrearDetalleVentaDto
                {
                    ProductoId = i.ProductoId,
                    Cantidad = i.Cantidad
                })
                .ToList()
        };

        var (venta, error) = await _api.PostAsync<CrearVentaDto, VentaDto>("api/Ventas", dto);

        // El carrito se vacía solo si la API confirmó la venta
        if (venta != null)
        {
            _carrito.Vaciar();
        }

        return (venta, error);
    }
}