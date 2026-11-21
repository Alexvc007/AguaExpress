using AguaExpress.App.ViewModels;

namespace AguaExpress.App.Vistas;

public partial class MisPedidosPage : ContentPage
{
    private readonly MisPedidosViewModel _vm;
    public MisPedidosPage(MisPedidosViewModel vm) { InitializeComponent(); BindingContext = _vm = vm; }
    protected override void OnAppearing() { base.OnAppearing(); _vm.CargarCommand.Execute(null); }
}
