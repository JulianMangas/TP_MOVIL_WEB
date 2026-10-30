using Microsoft.JSInterop;

namespace SistemaInventario.Web.Services
{
    public class TokenStorage
    {
        private const string Clave = "authToken";
        private readonly IJSRuntime _js;

        public TokenStorage(IJSRuntime js) => _js = js;

        public ValueTask<string?> ObtenerAsync() =>
            _js.InvokeAsync<string?>("localStorage.getItem", Clave);

        public ValueTask GuardarAsync(string token) =>
            _js.InvokeVoidAsync("localStorage.setItem", Clave, token);

        public ValueTask BorrarAsync() =>
            _js.InvokeVoidAsync("localStorage.removeItem", Clave);
    }
}