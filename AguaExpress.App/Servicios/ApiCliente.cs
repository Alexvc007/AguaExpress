using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AguaExpress.App.Configuracion;

namespace AguaExpress.App.Servicios;

public class ApiException(string mensaje) : Exception(mensaje);

public class ApiCliente(SesionServicio sesion)
{
    private readonly HttpClient _http = new() { BaseAddress = new Uri(AjustesApi.UrlBase), Timeout = TimeSpan.FromSeconds(30) };
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    public SesionServicio Sesion => sesion;

    private async Task<HttpResponseMessage> Core(HttpMethod metodo, string ruta, object? cuerpo)
    {
        var req = new HttpRequestMessage(metodo, ruta);
        if (sesion.Token is not null) req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", sesion.Token);
        if (cuerpo is not null) req.Content = JsonContent.Create(cuerpo, options: Json);

        HttpResponseMessage resp;
        try { resp = await _http.SendAsync(req); }
        catch (Exception) { throw new ApiException("No se pudo conectar con el servidor. Revisa tu conexión a internet."); }

        if (resp.IsSuccessStatusCode) return resp;
        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized && sesion.Token is not null)
        {
            sesion.Cerrar();
            await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync("//login"));
            throw new ApiException("Tu sesión expiró. Inicia sesión nuevamente.");
        }
        string mensaje = "Ocurrió un error inesperado.";
        try
        {
            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            if (doc.RootElement.TryGetProperty("mensaje", out var m)) mensaje = m.GetString() ?? mensaje;
        }
        catch { /* respuesta sin cuerpo JSON */ }
        throw new ApiException(mensaje);
    }

    public async Task<T> ObtenerAsync<T>(string ruta)
    {
        var r = await Core(HttpMethod.Get, ruta, null);
        return (await r.Content.ReadFromJsonAsync<T>(Json))!;
    }

    public async Task<T?> EnviarAsync<T>(HttpMethod metodo, string ruta, object? cuerpo = null)
    {
        var r = await Core(metodo, ruta, cuerpo);
        return await r.Content.ReadFromJsonAsync<T>(Json);
    }

    public async Task EnviarAsync(HttpMethod metodo, string ruta, object? cuerpo = null) => await Core(metodo, ruta, cuerpo);
}
