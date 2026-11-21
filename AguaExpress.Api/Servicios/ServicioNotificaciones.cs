using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace AguaExpress.Api.Servicios;

/// <summary>Envía notificaciones push con Firebase Cloud Messaging (FCM).</summary>
public class ServicioNotificaciones
{
    private readonly ILogger<ServicioNotificaciones> _log;
    private readonly bool _activo;

    public ServicioNotificaciones(IConfiguration cfg, ILogger<ServicioNotificaciones> log)
    {
        _log = log;
        var json = cfg["Firebase:CredencialesJson"];
        if (string.IsNullOrWhiteSpace(json))
        {
            _log.LogWarning("Firebase no configurado: las notificaciones push están desactivadas.");
            return;
        }
        if (FirebaseApp.DefaultInstance is null)
            FirebaseApp.Create(new AppOptions { Credential = GoogleCredential.FromJson(json) });
        _activo = true;
    }

    public async Task EnviarAsync(string? token, string titulo, string cuerpo, Dictionary<string, string>? datos = null)
    {
        if (!_activo || string.IsNullOrWhiteSpace(token)) return;
        try
        {
            await FirebaseMessaging.DefaultInstance.SendAsync(new Message
            {
                Token = token,
                Notification = new Notification { Title = titulo, Body = cuerpo },
                Data = datos,
                Android = new AndroidConfig { Priority = Priority.High }
            });
        }
        catch (Exception ex) { _log.LogError(ex, "No se pudo enviar la notificación"); }
    }

    public async Task EnviarAVariosAsync(IEnumerable<string?> tokens, string titulo, string cuerpo)
    {
        foreach (var t in tokens.Where(t => !string.IsNullOrWhiteSpace(t)).Distinct())
            await EnviarAsync(t, titulo, cuerpo);
    }
}
