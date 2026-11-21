using System.Globalization;

namespace AguaExpress.App.Servicios;

/// <summary>Abre la navegación con Deep Linking nativo (sin API ni costos).</summary>
public static class MapasServicio
{
    public static async Task NavegarAsync(double lat, double lng)
    {
        var la = lat.ToString(CultureInfo.InvariantCulture);
        var lo = lng.ToString(CultureInfo.InvariantCulture);
        // 1) Google Maps con la ruta ya trazada en modo conducción.
        // 2) geo: -> el mapa predeterminado del usuario (Waze, Maps, etc.).
        // 3) URL web como último recurso.
        var intentos = new[]
        {
            $"google.navigation:q={la},{lo}&mode=d",
            $"geo:{la},{lo}?q={la},{lo}",
            $"https://www.google.com/maps/dir/?api=1&destination={la},{lo}&travelmode=driving"
        };
        foreach (var uri in intentos)
        {
            try { if (await Launcher.Default.TryOpenAsync(new Uri(uri))) return; } catch { }
        }
        await Shell.Current.DisplayAlert("Mapas", "No se encontró una app de mapas en el teléfono.", "Aceptar");
    }
}
