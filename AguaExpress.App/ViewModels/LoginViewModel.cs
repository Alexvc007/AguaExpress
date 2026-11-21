using AguaExpress.App.Modelos;
using AguaExpress.App.Servicios;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AguaExpress.App.ViewModels;

public partial class LoginViewModel(ApiCliente api, NotificacionesPush push) : BaseViewModel(api)
{
    [ObservableProperty] private string correo = "";
    [ObservableProperty] private string contrasena = "";

    public async Task IntentarSesionGuardadaAsync()
    {
        var s = await Api.Sesion.CargarAsync();
        if (s is null) return;
        await Shell.Current.GoToAsync(SesionServicio.RutaInicial(s.Rol));
        _ = push.RegistrarAsync();
    }

    [RelayCommand]
    private Task Ingresar() => Ejecutar(async () =>
    {
        if (string.IsNullOrWhiteSpace(Correo) || string.IsNullOrWhiteSpace(Contrasena))
        { await Alerta("Aviso", "Ingresa tu correo y contraseña."); return; }

        var s = await Api.EnviarAsync<SesionDto>(HttpMethod.Post, "auth/login", new { Correo, Contrasena });
        await Api.Sesion.GuardarAsync(s!);
        Contrasena = "";
        await Shell.Current.GoToAsync(SesionServicio.RutaInicial(s!.Rol));
        _ = push.RegistrarAsync();
    });

    [RelayCommand] private Task IrARegistro() => Shell.Current.GoToAsync("registro");
}
