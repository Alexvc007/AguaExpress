using AguaExpress.App.ViewModels;

namespace AguaExpress.App.Vistas;

public partial class NuevoPedidoPage : ContentPage
{
    private readonly NuevoPedidoViewModel _vm;
    public NuevoPedidoPage(NuevoPedidoViewModel vm) { InitializeComponent(); BindingContext = _vm = vm; }
    protected override void OnAppearing() { base.OnAppearing(); _vm.CargarCommand.Execute(null); }
}
