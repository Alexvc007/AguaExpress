using AguaExpress.App.ViewModels;

namespace AguaExpress.App.Vistas;

public partial class EntregasPage : ContentPage
{
    private readonly EntregasViewModel _vm;
    public EntregasPage(EntregasViewModel vm) { InitializeComponent(); BindingContext = _vm = vm; }
    protected override void OnAppearing() { base.OnAppearing(); _vm.CargarCommand.Execute(null); }
}
