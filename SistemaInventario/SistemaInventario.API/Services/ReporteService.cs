using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaInventario.API.Data;

namespace SistemaInventario.API.Services
{
    public class ReporteService : IReporteService
    {
        private static readonly CultureInfo Es = new("es-AR");

        // Describe una columna de tabla: ancho fijo (si es 0, reparte el espacio según Peso)
        private record Col(string Titulo, float Ancho = 0, bool Derecha = false, float Peso = 1);

        private readonly AppDbContext _context;

        public ReporteService(AppDbContext context)
        {
            _context = context;
        }

        // ---------- Reporte diario ----------

        public async Task<(byte[] Contenido, string NombreArchivo)> GenerarDiarioAsync(DateTime? fecha)
        {
            var dia = (fecha ?? DateTime.Now).Date;
            var siguiente = dia.AddDays(1);

            var salidas = await _context.Ventas
                .Where(v => v.Fecha >= dia && v.Fecha < siguiente)
                .SelectMany(v => v.Detalles.Select(d => new
                {
                    v.Id,
                    Cliente = v.Cliente != null ? v.Cliente.Nombre + " " + v.Cliente.Apellido : "-",
                    Producto = d.Producto != null ? d.Producto.Nombre : "-",
                    d.Cantidad,
                    Precio = d.PrecioVenta
                }))
                .OrderBy(x => x.Id)
                .ToListAsync();

            var entradas = await _context.Ingresos
                .Where(i => i.Fecha >= dia && i.Fecha < siguiente)
                .SelectMany(i => i.Detalles.Select(d => new
                {
                    i.Id,
                    Proveedor = i.Proveedor != null ? i.Proveedor.Nombre : "-",
                    Producto = d.Producto != null ? d.Producto.Nombre : "-",
                    d.Cantidad,
                    Precio = d.PrecioCompra
                }))
                .OrderBy(x => x.Id)
                .ToListAsync();

            var stock = await _context.Productos
                .OrderBy(p => p.Nombre)
                .Select(p => new
                {
                    p.Nombre,
                    Categoria = p.Categoria != null ? p.Categoria.Nombre : "-",
                    p.Precio,
                    p.Stock
                })
                .ToListAsync();

            var totalVendido = salidas.Sum(s => s.Cantidad * s.Precio);
            var totalComprado = entradas.Sum(e => e.Cantidad * e.Precio);

            var contenido = Armar(
                "Reporte diario",
                $"Fecha: {dia:dd/MM/yyyy}",
                horizontal: false,
                col =>
                {
                    col.Item().Text($"Ventas del día: {salidas.Sum(s => s.Cantidad)} unidades por $ {totalVendido.ToString("N2", Es)}");
                    col.Item().Text($"Ingresos del día: {entradas.Sum(e => e.Cantidad)} unidades por $ {totalComprado.ToString("N2", Es)}");
                    col.Item().Text($"Stock actual: {stock.Sum(s => s.Stock)} unidades en {stock.Count} productos");

                    Seccion(col, "Salidas del día (ventas)",
                        new[]
                        {
                            new Col("N°", 35), new Col("Cliente"), new Col("Producto"),
                            new Col("Cant.", 35, true), new Col("Precio", 60, true), new Col("Subtotal", 70, true)
                        },
                        salidas.Select(s => new[]
                        {
                            s.Id.ToString(), s.Cliente, s.Producto, s.Cantidad.ToString(),
                            s.Precio.ToString("N2", Es), (s.Cantidad * s.Precio).ToString("N2", Es)
                        }).ToList(),
                        new[] { "", "TOTAL", "", salidas.Sum(s => s.Cantidad).ToString(), "", totalVendido.ToString("N2", Es) });

                    Seccion(col, "Entradas del día (ingresos de mercadería)",
                        new[]
                        {
                            new Col("N°", 35), new Col("Proveedor"), new Col("Producto"),
                            new Col("Cant.", 35, true), new Col("Costo", 60, true), new Col("Subtotal", 70, true)
                        },
                        entradas.Select(e => new[]
                        {
                            e.Id.ToString(), e.Proveedor, e.Producto, e.Cantidad.ToString(),
                            e.Precio.ToString("N2", Es), (e.Cantidad * e.Precio).ToString("N2", Es)
                        }).ToList(),
                        new[] { "", "TOTAL", "", entradas.Sum(e => e.Cantidad).ToString(), "", totalComprado.ToString("N2", Es) });

                    Seccion(col, "Stock actual",
                        new[]
                        {
                            new Col("Producto"), new Col("Categoría"),
                            new Col("Precio", 60, true), new Col("Stock", 40, true), new Col("Valor", 75, true)
                        },
                        stock.Select(s => new[]
                        {
                            s.Nombre, s.Categoria, s.Precio.ToString("N2", Es),
                            s.Stock.ToString(), (s.Precio * s.Stock).ToString("N2", Es)
                        }).ToList(),
                        new[] { "TOTAL", "", "", stock.Sum(s => s.Stock).ToString(),
                                stock.Sum(s => s.Precio * s.Stock).ToString("N2", Es) });
                });

            return (contenido, $"reporte-diario-{dia:yyyyMMdd}.pdf");
        }

        // ---------- Auditoría ----------

        public async Task<(byte[] Contenido, string NombreArchivo)> GenerarAuditoriaAsync()
        {
            var datos = await _context.Auditorias
                .OrderByDescending(a => a.Id)
                .Take(200)
                .Select(a => new
                {
                    a.Fecha,
                    Usuario = a.Usuario != null ? a.Usuario.Nombre : "-",
                    a.Accion,
                    a.Entidad,
                    a.Detalle
                })
                .ToListAsync();

            var contenido = Armar(
                "Auditoría",
                "Últimos 200 registros, del más reciente al más antiguo",
                horizontal: false,
                col => Seccion(col, "",
                    new[]
                    {
                        new Col("Fecha", 78), new Col("Usuario"), new Col("Acción", 62),
                        new Col("Entidad", 62), new Col("Detalle", Peso: 2)
                    },
                    datos.Select(a => new[]
                    {
                        a.Fecha.ToString("dd/MM/yyyy HH:mm"), a.Usuario, a.Accion, a.Entidad, a.Detalle ?? ""
                    }).ToList()));

            return (contenido, $"auditoria-{DateTime.Now:yyyyMMdd-HHmm}.pdf");
        }

        // ---------- Registro de errores ----------

        public async Task<(byte[] Contenido, string NombreArchivo)> GenerarErroresAsync()
        {
            var datos = await _context.LogsError
                .OrderByDescending(e => e.Id)
                .Take(100)
                .Select(e => new { e.Fecha, e.Metodo, e.Ruta, e.Mensaje, e.Detalle })
                .ToListAsync();

            var contenido = Armar(
                "Registro de errores",
                "Últimos 100 errores; el detalle técnico se recorta a 300 caracteres",
                horizontal: true,
                col => Seccion(col, "",
                    new[]
                    {
                        new Col("Fecha", 78), new Col("Método", 45), new Col("Ruta", Peso: 1.5f),
                        new Col("Mensaje", Peso: 2), new Col("Detalle", Peso: 3)
                    },
                    datos.Select(e => new[]
                    {
                        e.Fecha.ToString("dd/MM/yyyy HH:mm"), e.Metodo, e.Ruta, e.Mensaje, Cortar(e.Detalle, 300)
                    }).ToList()));

            return (contenido, $"errores-{DateTime.Now:yyyyMMdd-HHmm}.pdf");
        }

        // ---------- Diseño común ----------

        private static string Cortar(string texto, int max) =>
            texto.Length <= max ? texto : texto[..max] + "...";

        private static byte[] Armar(
            string titulo, string subtitulo, bool horizontal, Action<ColumnDescriptor> contenido)
        {
            return Document.Create(doc => doc.Page(pagina =>
            {
                pagina.Size(horizontal ? PageSizes.A4.Landscape() : PageSizes.A4);
                pagina.Margin(30);
                pagina.DefaultTextStyle(x => x.FontSize(9));

                pagina.Header().Column(col =>
                {
                    col.Item().Text(titulo).FontSize(16).Bold();
                    col.Item().Text(subtitulo).FontColor(Colors.Grey.Darken1);
                    col.Item().Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}")
                        .FontColor(Colors.Grey.Darken1);
                });

                pagina.Content().PaddingVertical(10).Column(contenido);

                pagina.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Página ");
                    t.CurrentPageNumber();
                    t.Span(" de ");
                    t.TotalPages();
                });
            })).GeneratePdf();
        }

        private static void Seccion(
            ColumnDescriptor col, string titulo, Col[] columnas,
            List<string[]> filas, string[]? pie = null)
        {
            if (titulo != "")
            {
                col.Item().PaddingTop(12).PaddingBottom(4).Text(titulo).FontSize(12).Bold();
            }

            if (filas.Count == 0)
            {
                col.Item().Text("Sin registros.").FontColor(Colors.Grey.Darken1);
                return;
            }

            col.Item().Table(tabla =>
            {
                tabla.ColumnsDefinition(c =>
                {
                    foreach (var k in columnas)
                    {
                        if (k.Ancho > 0) c.ConstantColumn(k.Ancho);
                        else c.RelativeColumn(k.Peso);
                    }
                });

                tabla.Header(h =>
                {
                    foreach (var k in columnas)
                    {
                        var celda = h.Cell().Element(Encabezado);
                        if (k.Derecha) celda = celda.AlignRight();
                        celda.Text(k.Titulo);
                    }
                });

                foreach (var fila in filas) Fila(tabla, columnas, fila, false);
                if (pie != null) Fila(tabla, columnas, pie, true);
            });
        }

        private static void Fila(TableDescriptor tabla, Col[] columnas, string[] valores, bool negrita)
        {
            for (var i = 0; i < columnas.Length; i++)
            {
                var celda = tabla.Cell().Element(Celda);
                if (columnas[i].Derecha) celda = celda.AlignRight();
                var texto = celda.Text(valores[i]);
                if (negrita) texto.Bold();
            }
        }

        private static IContainer Encabezado(IContainer c) =>
            c.Background(Colors.Grey.Lighten3).Padding(4).DefaultTextStyle(x => x.Bold());

        private static IContainer Celda(IContainer c) =>
            c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).PaddingHorizontal(4);
    }
}