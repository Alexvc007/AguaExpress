using AguaExpress.App.Modelos;

namespace AguaExpress.App.Servicios;

public class SesionServicio
{
    public SesionDto? Actual { get; private set; }
    public string? Token => Actual?.Token;

    public async Task<SesionDto?> CargarAsync()
    {
        try
        {
            var token = await SecureStorage.Default.GetAsync("token");
            if (string.IsNullOrEmpty(token)) return null;
            Actual = new SesionDto
            {
                Token = token, Nombre = Preferences.Get("nombre", ""), Rol = Preferences.Get("rol", ""), Id = Preferences.Get("id", 0)
            };
            return Actual;
        }
        catch { return null; }
    }

    public async Task GuardarAsync(SesionDto s)
    {
        Actual = s;
        Preferences.Set("nombre", s.Nombre); Preferences.Set("rol", s.Rol); Preferences.Set("id", s.Id);
        await SecureStorage.Default.SetAsync("token", s.Token);
    }

    public void Cerrar()
    {
        Actual = null;
        SecureStorage.Default.Remove("token");
        Preferences.Clear();
    }

    public static string RutaInicial(string rol) => rol switch
    {
        "Administrador" => "//admin", "Vendedor" => "//vendedor", _ => "//cliente"
    };
}
