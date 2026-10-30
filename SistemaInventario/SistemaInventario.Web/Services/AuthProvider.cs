using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace SistemaInventario.Web.Services
{
    public class AuthProvider : AuthenticationStateProvider
    {
        private readonly TokenStorage _storage;

        private static AuthenticationState Anonimo =>
            new(new ClaimsPrincipal(new ClaimsIdentity()));

        public AuthProvider(TokenStorage storage) => _storage = storage;

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _storage.ObtenerAsync();

            if (string.IsNullOrWhiteSpace(token))
                return Anonimo;

            try
            {
                var claims = JwtParser.Leer(token);
                var vence = JwtParser.Vencimiento(claims);

                if (vence == null || vence <= DateTimeOffset.UtcNow)
                {
                    await _storage.BorrarAsync();
                    return Anonimo;
                }

                var identidad = new ClaimsIdentity(claims, "jwt", ClaimTypes.Name, ClaimTypes.Role);
                return new AuthenticationState(new ClaimsPrincipal(identidad));
            }
            catch
            {
                await _storage.BorrarAsync();
                return Anonimo;
            }
        }

        public async Task IniciarSesionAsync(string token)
        {
            await _storage.GuardarAsync(token);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task CerrarSesionAsync()
        {
            await _storage.BorrarAsync();
            NotifyAuthenticationStateChanged(Task.FromResult(Anonimo));
        }
    }
}