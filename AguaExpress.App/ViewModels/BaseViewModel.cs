using AguaExpress.App.Servicios;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AguaExpress.App.ViewModels;

public abstract partial class BaseViewModel(ApiCliente api) : ObservableObject
{
    protected ApiCliente Api { get; } = api;

    [ObservableProperty] private bool ocupado;

    public string NombreUsuario => Api.Sesion.Actual?.Nombre ?? "";

    protected async Task Ejecutar(Func<Task> accion)
    {
        if (Ocupado) return;
        try { Ocupado = true; await accion(); }
        catch (ApiException ex) { await Alerta("Aviso", ex.Message); }
        catch (Exception) { await Alerta("Error", "No se pudo completar la operación. Intenta nuevamente."); }
        finally { Ocupado = false; }
    }

    protected static Task Alerta(string titulo, string mensaje) => Shell.Current.DisplayAlert(titulo, mensaje, "Aceptar");
    protected static Task<bool> Confirmar(string titulo, string mensaje) => Shell.Current.DisplayAlert(titulo, mensaje, "Sí", "No");

    [RelayCommand]
    private async Task CerrarSesion()
    {
        if (!await Confirmar("Cerrar sesión", "¿Deseas salir de tu cuenta?")) return;
        Api.Sesion.Cerrar();
        await Shell.Current.GoToAsync("//login");
    }
}
