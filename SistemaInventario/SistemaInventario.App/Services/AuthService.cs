using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SistemaInventario.App.Models;

namespace SistemaInventario.App.Services;

public class AuthService
{
    private readonly HttpClient _http;

    public AuthService(HttpClient http)
    {
        _http = http;
    }

    public string? Token { get; private set; }
    public string? Nombre { get; private set; }
    public string? Rol { get; private set; }
    public int UsuarioId { get; private set; }

    public bool EstaLogueado => !string.IsNullOrEmpty(Token);

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

            Token = datos.Token;
            Nombre = datos.Nombre;
            Rol = datos.Rol;
            UsuarioId = datos.UsuarioId;

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", Token);

            await SecureStorage.Default.SetAsync("token", Token);

            return (true, null);
        }
        catch (Exception)
        {
            return (false, "No se pudo conectar con el servidor. Revisá tu conexión.");
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
    }
}
