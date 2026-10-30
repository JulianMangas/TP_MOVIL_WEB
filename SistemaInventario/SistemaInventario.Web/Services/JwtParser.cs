using System.Security.Claims;
using System.Text.Json;

namespace SistemaInventario.Web.Services
{
    public static class JwtParser
    {
        public static List<Claim> Leer(string jwt)
        {
            var payload = jwt.Split('.')[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');

            using var doc = JsonDocument.Parse(Convert.FromBase64String(payload));
            var claims = new List<Claim>();

            foreach (var p in doc.RootElement.EnumerateObject())
            {
                var tipo = p.Name switch
                {
                    "role" or ClaimTypes.Role => ClaimTypes.Role,
                    "name" or ClaimTypes.Name => ClaimTypes.Name,
                    "nameid" or ClaimTypes.NameIdentifier => ClaimTypes.NameIdentifier,
                    _ => p.Name
                };

                if (p.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var v in p.Value.EnumerateArray())
                        claims.Add(new Claim(tipo, v.ToString()));
                }
                else
                {
                    claims.Add(new Claim(tipo, p.Value.ToString()));
                }
            }

            return claims;
        }

        public static DateTimeOffset? Vencimiento(IEnumerable<Claim> claims)
        {
            var exp = claims.FirstOrDefault(c => c.Type == "exp")?.Value;
            return long.TryParse(exp, out var s) ? DateTimeOffset.FromUnixTimeSeconds(s) : null;
        }
    }
}