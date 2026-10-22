using Microsoft.Extensions.Logging;
using SistemaInventario.App.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace SistemaInventario.App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            const string urlApi = "https://sistemainventario.runasp.net/";

            builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri(urlApi) });
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<ApiClient>();
            builder.Services.AddSingleton<CatalogoService>();
            builder.Services.AddSingleton<CarritoService>();
            builder.Services.AddSingleton<CompraService>();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddSingleton<AuthenticationStateProvider, SesionAuthStateProvider>();
            return builder.Build();
        }
    }
}
