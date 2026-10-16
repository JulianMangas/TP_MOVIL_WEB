using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace SistemaInventario.App.Services;

public class SesionAuthStateProvider : AuthenticationStateProvider
{
    private readonly AuthService _auth;

    public SesionAuthStateProvider(AuthService auth)
    {
        _auth = auth;

        // Cuando alguien entra o sale, se avisa a toda la interfaz
        _auth.SesionCambio += () =>
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        await _auth.RestaurarSesionAsync();

        if (!_auth.EstaLogueado)
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var identidad = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _auth.UsuarioId.ToString()),
            new Claim(ClaimTypes.Name, _auth.Nombre ?? ""),
            new Claim(ClaimTypes.Role, _auth.Rol ?? "")
        }, authenticationType: "jwt");

        return new AuthenticationState(new ClaimsPrincipal(identidad));
    }
}