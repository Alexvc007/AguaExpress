namespace AguaExpress.App.Servicios;

public class NotificacionesPush(ApiCliente api)
{
    // Pendiente: registrar aquí el token FCM cuando activemos las notificaciones.
    public Task RegistrarAsync() => Task.CompletedTask;
}