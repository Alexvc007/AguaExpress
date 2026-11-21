using AguaExpress.App.ViewModels;

namespace AguaExpress.App.Vistas;

public partial class TrabajadoresPage : ContentPage
{
    private readonly TrabajadoresViewModel _vm;
    public TrabajadoresPage(TrabajadoresViewModel vm) { InitializeComponent(); BindingContext = _vm = vm; }
    protected override void OnAppearing() { base.OnAppearing(); _vm.CargarCommand.Execute(null); }
}
