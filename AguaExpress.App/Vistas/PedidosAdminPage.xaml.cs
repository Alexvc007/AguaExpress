using AguaExpress.App.ViewModels;

namespace AguaExpress.App.Vistas;

public partial class PedidosAdminPage : ContentPage
{
    private readonly PedidosAdminViewModel _vm;
    public PedidosAdminPage(PedidosAdminViewModel vm) { InitializeComponent(); BindingContext = _vm = vm; }
    protected override void OnAppearing() { base.OnAppearing(); _vm.CargarCommand.Execute(null); }
}
