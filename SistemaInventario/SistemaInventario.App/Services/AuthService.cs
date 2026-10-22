using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using SistemaInventario.App.Models;

namespace SistemaInventario.App.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private bool _sesionRestaurada;

    public AuthService(HttpClient http)
    {
        _http = http;
    }

    public string? Token { get; private set; }
    public string? Nombre { get; private set; }
    public string? Rol { get; private set; }
    public int UsuarioId { get; private set; }

    public bool EstaLogueado => !string.IsNullOrEmpty(Token);

    // Se dispara cuando alguien inicia o cierra sesión
    public event Action? SesionCambio;

    public async Task<(bool Exito, string? Error)> LoginAsync(string email, string password)
    {
        try
        {
            var respuesta = await _http.PostAsJsonAsync(
                "api/Auth/login",
                new LoginDto { Email = email, Password = password });

            if (!respuesta.IsSuccessStatusCode)
            {
                var error = await respuesta.Content.ReadFromJsonAsync<ErrorRespuesta>();
                return (false, error?.Mensaje ?? "No se pudo iniciar sesión.");
            }

            var datos = await respuesta.Content.ReadFromJsonAsync<LoginRespuestaDto>();

            if (datos == null)
            {
                return (false, "Respuesta inválida del servidor.");
            }

            EstablecerSesion(datos.Token, datos.Nombre, datos.Rol, datos.UsuarioId);

            await SecureStorage.Default.SetAsync("token", datos.Token);
            await SecureStorage.Default.SetAsync("nombre", datos.Nombre);
            await SecureStorage.Default.SetAsync("rol", datos.Rol);
            await SecureStorage.Default.SetAsync("usuarioId", datos.UsuarioId.ToString());

            SesionCambio?.Invoke();

            return (true, null);
        }
        catch (Exception)
        {
            return (false, "No se pudo conectar con el servidor. Revisá tu conexión.");
        }
    }

    public async Task RestaurarSesionAsync()
    {
        if (_sesionRestaurada)
        {
            return;
        }

        _sesionRestaurada = true;

        try
        {
            var token = await SecureStorage.Default.GetAsync("token");

            if (string.IsNullOrEmpty(token))
            {
                return;
            }

            var expiracion = ObtenerExpiracion(token);

            if (expiracion == null || expiracion <= DateTime.UtcNow)
            {
                SecureStorage.Default.RemoveAll();
                return;
            }

            var nombre = await SecureStorage.Default.GetAsync("nombre");
            var rol = await SecureStorage.Default.GetAsync("rol");
            var idTexto = await SecureStorage.Default.GetAsync("usuarioId");

            int.TryParse(idTexto, out var usuarioId);

            EstablecerSesion(token, nombre ?? "", rol ?? "", usuarioId);
        }
        catch (Exception)
        {
            // Si el almacenamiento seguro falla, se arranca sin sesión
        }
    }

    public void Logout()
    {
        Token = null;
        Nombre = null;
        Rol = null;
        UsuarioId = 0;

        _http.DefaultRequestHeaders.Authorization = null;
        SecureStorage.Default.RemoveAll();

        SesionCambio?.Invoke();
    }

    private void EstablecerSesion(string token, string nombre, string rol, int usuarioId)
    {
        Token = token;
        Nombre = nombre;
        Rol = rol;
        UsuarioId = usuarioId;

        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    // Lee el campo "exp" (expiración) del payload del JWT
    private static DateTime? ObtenerExpiracion(string token)
    {
        try
        {
            var partes = token.Split('.');

            if (partes.Length != 3)
            {
                return null;
            }

            var payload = partes[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');

            var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));

            using var documento = JsonDocument.Parse(json);

            if (documento.RootElement.TryGetProperty("exp", out var exp))
            {
                return DateTimeOffset.FromUnixTimeSeconds(exp.GetInt64()).UtcDateTime;
            }
        }
        catch (Exception)
        {
        }

        return null;
    }
}