using System.Net;
using System.Net.Http.Json;
using SistemaInventario.App.Models;

namespace SistemaInventario.App.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly AuthService _auth;

    public ApiClient(HttpClient http, AuthService auth)
    {
        _http = http;
        _auth = auth;
    }

    public async Task<(T? Datos, string? Error)> GetAsync<T>(string url)
    {
        try
        {
            var respuesta = await _http.GetAsync(url);

            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Token vencido o inválido: se cierra la sesión y las
                // pantallas protegidas redirigen solas al login
                _auth.Logout();
                return (default, "Tu sesión expiró. Iniciá sesión nuevamente.");
            }

            if (!respuesta.IsSuccessStatusCode)
            {
                return (default, await LeerErrorAsync(respuesta));
            }

            var datos = await respuesta.Content.ReadFromJsonAsync<T>();

            return (datos, null);
        }
        catch (Exception)
        {
            return (default, "No se pudo conectar con el servidor. Revisá tu conexión.");
        }
    }

    private static async Task<string> LeerErrorAsync(HttpResponseMessage respuesta)
    {
        try
        {
            var error = await respuesta.Content.ReadFromJsonAsync<ErrorRespuesta>();

            if (!string.IsNullOrEmpty(error?.Mensaje))
            {
                return error.Mensaje;
            }
        }
        catch (Exception)
        {
        }

        return $"Error {(int)respuesta.StatusCode} del servidor.";
    }
}