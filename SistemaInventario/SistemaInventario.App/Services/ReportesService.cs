using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Storage;

namespace SistemaInventario.App.Services;

public class ReportesService
{
    private readonly ApiClient _api;

    public ReportesService(ApiClient api)
    {
        _api = api;
    }

    // Pide el PDF a la API y lo guarda en el celular.
    // Devuelve (mensaje de éxito, error): uno de los dos viene vacío.
    public async Task<(string? Mensaje, string? Error)> DescargarAsync(string ruta, string prefijo)
    {
        var (bytes, error) = await _api.DescargarAsync($"api/Reportes/{ruta}");

        if (bytes == null)
        {
            return (null, error);
        }

        var nombre = $"{prefijo}-{DateTime.Now:yyyyMMdd-HHmm}.pdf";

        try
        {
            return (await GuardarAsync(bytes, nombre), null);
        }
        catch (Exception)
        {
            return (null, "No se pudo guardar el archivo en el celular.");
        }
    }

    private static async Task<string> GuardarAsync(byte[] bytes, string nombre)
    {
#if ANDROID
        // Android 10+ (API 29): se guarda en la carpeta pública Descargas
        // mediante MediaStore. No requiere permisos.
        if (OperatingSystem.IsAndroidVersionAtLeast(29))
        {
            var resolver = Android.App.Application.Context.ContentResolver!;

            var valores = new Android.Content.ContentValues();
            valores.Put(Android.Provider.MediaStore.IMediaColumns.DisplayName, nombre);
            valores.Put(Android.Provider.MediaStore.IMediaColumns.MimeType, "application/pdf");
            valores.Put(Android.Provider.MediaStore.IMediaColumns.RelativePath,
                Android.OS.Environment.DirectoryDownloads);

            var uri = resolver.Insert(
                Android.Provider.MediaStore.Downloads.ExternalContentUri!, valores)
                ?? throw new IOException("No se pudo crear el archivo.");

            using (var salida = resolver.OpenOutputStream(uri)!)
            {
                await salida.WriteAsync(bytes);
            }

            AbrirPdf(uri);

            return $"Guardado en Descargas: {nombre}";
        }
#endif
        // Versiones anteriores: se guarda en caché y se abre el menú de compartir
        var ruta = Path.Combine(FileSystem.CacheDirectory, nombre);
        await File.WriteAllBytesAsync(ruta, bytes);

        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = nombre,
            File = new ShareFile(ruta)
        });

        return $"Archivo generado: {nombre}";
    }

#if ANDROID
    private static void AbrirPdf(Android.Net.Uri uri)
    {
        try
        {
            var intent = new Android.Content.Intent(Android.Content.Intent.ActionView);
            intent.SetDataAndType(uri, "application/pdf");
            intent.AddFlags(
                Android.Content.ActivityFlags.GrantReadUriPermission |
                Android.Content.ActivityFlags.NewTask);

            Android.App.Application.Context.StartActivity(intent);
        }
        catch (Android.Content.ActivityNotFoundException)
        {
            // No hay visor de PDF instalado: el archivo igual quedó guardado
        }
    }
#endif
}