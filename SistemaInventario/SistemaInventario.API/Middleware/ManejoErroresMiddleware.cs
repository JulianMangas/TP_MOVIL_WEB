using SistemaInventario.API.Data;
using SistemaInventario.API.Exceptions;
using SistemaInventario.API.Models;
using System.Security.Claims;

namespace SistemaInventario.API.Middleware
{
    public class ManejoErroresMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ManejoErroresMiddleware> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public ManejoErroresMiddleware(
            RequestDelegate next,
            ILogger<ManejoErroresMiddleware> logger,
            IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ReglaDeNegocioException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { mensaje = ex.Message });
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                // El cliente cortó la conexión: no es un error del sistema
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en {Metodo} {Ruta}",
                    context.Request.Method, context.Request.Path);

                await GuardarErrorAsync(context, ex);

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new
                {
                    mensaje = "Ocurrió un error inesperado. Intentá nuevamente más tarde."
                });
            }
        }

        private async Task GuardarErrorAsync(HttpContext context, Exception ex)
        {
            try
            {
                // Scope nuevo: el DbContext del request puede haber quedado
                // en mal estado justamente por el error que estamos registrando
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                int? usuarioId = null;
                var claim = context.User.FindFirst(ClaimTypes.NameIdentifier);

                if (claim != null && int.TryParse(claim.Value, out var id))
                {
                    usuarioId = id;
                }

                var ruta = context.Request.Path.Value ?? "";
                var mensaje = ex.Message;

                db.LogsError.Add(new LogError
                {
                    Fecha = DateTime.Now,
                    UsuarioId = usuarioId,
                    Metodo = context.Request.Method,
                    Ruta = ruta.Length > 300 ? ruta[..300] : ruta,
                    Mensaje = mensaje.Length > 500 ? mensaje[..500] : mensaje,
                    Detalle = ex.ToString()
                });

                await db.SaveChangesAsync();
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "No se pudo guardar el error en la base de datos");
            }
        }
    }
}