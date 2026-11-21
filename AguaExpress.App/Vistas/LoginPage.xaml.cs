using AguaExpress.App.ViewModels;

namespace AguaExpress.App.Vistas;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _vm;
    public LoginPage(LoginViewModel vm) { InitializeComponent(); BindingContext = _vm = vm; }
    protected override async void OnAppearing() { base.OnAppearing(); await _vm.IntentarSesionGuardadaAsync(); }
}
