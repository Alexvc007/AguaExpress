using AguaExpress.App.ViewModels;

namespace AguaExpress.App.Vistas;

public partial class RegistroPage : ContentPage
{
    public RegistroPage(RegistroViewModel vm) { InitializeComponent(); BindingContext = vm; }
}
