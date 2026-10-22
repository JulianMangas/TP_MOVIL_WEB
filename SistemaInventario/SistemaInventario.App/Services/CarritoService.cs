using SistemaInventario.App.Models;

namespace SistemaInventario.App.Services;

public class CarritoService
{
    private readonly List<ItemCarrito> _items = new();

    public CarritoService(AuthService auth)
    {
        // El carrito es de una persona: si cierra sesión, se vacía
        auth.SesionCambio += () =>
        {
            if (!auth.EstaLogueado)
            {
                Vaciar();
            }
        };
    }

    public IReadOnlyList<ItemCarrito> Items => _items;

    public int CantidadTotal => _items.Sum(i => i.Cantidad);

    public decimal Total => _items.Sum(i => i.Subtotal);

    // Avisa a quien esté mostrando el carrito (por ejemplo, el menú)
    public event Action? Cambio;

    public int CantidadEnCarrito(int productoId)
        => _items.FirstOrDefault(i => i.ProductoId == productoId)?.Cantidad ?? 0;

    public bool Agregar(ProductoDto producto)
    {
        if (producto.Stock <= 0)
        {
            return false;
        }

        var item = _items.FirstOrDefault(i => i.ProductoId == producto.Id);

        if (item == null)
        {
            _items.Add(new ItemCarrito
            {
                ProductoId = producto.Id,
                Nombre = producto.Nombre,
                Precio = producto.Precio,
                Stock = producto.Stock,
                ImagenUrl = producto.ImagenUrl,
                Cantidad = 1
            });
        }
        else
        {
            if (item.Cantidad >= producto.Stock)
            {
                return false;
            }

            item.Cantidad++;
            item.Stock = producto.Stock;
            item.Precio = producto.Precio;
        }

        Cambio?.Invoke();
        return true;
    }

    public void CambiarCantidad(int productoId, int diferencia)
    {
        var item = _items.FirstOrDefault(i => i.ProductoId == productoId);

        if (item == null)
        {
            return;
        }

        var nueva = item.Cantidad + diferencia;

        if (nueva <= 0)
        {
            _items.Remove(item);
        }
        else if (nueva <= item.Stock)
        {
            item.Cantidad = nueva;
        }
        else
        {
            return;
        }

        Cambio?.Invoke();
    }

    public void Quitar(int productoId)
    {
        _items.RemoveAll(i => i.ProductoId == productoId);
        Cambio?.Invoke();
    }

    public void Vaciar()
    {
        _items.Clear();
        Cambio?.Invoke();
    }
}