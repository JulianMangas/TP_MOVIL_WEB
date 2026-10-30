using System.Net.Http.Headers;

namespace SistemaInventario.Web.Services
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly TokenStorage _storage;

        public AuthHeaderHandler(TokenStorage storage) => _storage = storage;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _storage.ObtenerAsync();

            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}