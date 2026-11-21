using AguaExpress.App.Modelos;
using AguaExpress.App.Servicios;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AguaExpress.App.ViewModels;

public partial class RegistroViewModel(ApiCliente api, NotificacionesPush push) : BaseViewModel(api)
{
    [ObservableProperty] private string nombreCompleto = "";
    [ObservableProperty] private string correo = "";
    [ObservableProperty] private string telefono = "";
    [ObservableProperty] private string contrasena = "";

    [RelayCommand]
    private Task Registrar() => Ejecutar(async () =>
    {
        if (string.IsNullOrWhiteSpace(NombreCompleto) || string.IsNullOrWhiteSpace(Correo) ||
            string.IsNullOrWhiteSpace(Telefono) || Contrasena.Length < 6)
        { await Alerta("Aviso", "Completa todos los campos. La contraseña debe tener mínimo 6 caracteres."); return; }

        var s = await Api.EnviarAsync<SesionDto>(HttpMethod.Post, "auth/registro",
            new { NombreCompleto, Correo, Telefono, Contrasena });
        await Api.Sesion.GuardarAsync(s!);
        await Shell.Current.GoToAsync(SesionServicio.RutaInicial(s!.Rol));
        _ = push.RegistrarAsync();
    });
}
