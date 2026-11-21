using AguaExpress.App.ViewModels;

namespace AguaExpress.App.Vistas;

public partial class InventarioPage : ContentPage
{
    private readonly InventarioViewModel _vm;
    public InventarioPage(InventarioViewModel vm) { InitializeComponent(); BindingContext = _vm = vm; }
    protected override void OnAppearing() { base.OnAppearing(); _vm.CargarCommand.Execute(null); }
}
