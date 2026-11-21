using AguaExpress.App.ViewModels;

namespace AguaExpress.App.Vistas;

public partial class PanelPage : ContentPage
{
    private readonly PanelViewModel _vm;
    public PanelPage(PanelViewModel vm) { InitializeComponent(); BindingContext = _vm = vm; }
    protected override void OnAppearing() { base.OnAppearing(); _vm.CargarCommand.Execute(null); }
}
