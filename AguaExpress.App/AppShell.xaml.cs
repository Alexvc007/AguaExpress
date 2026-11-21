using AguaExpress.App.Vistas;

namespace AguaExpress.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("registro", typeof(RegistroPage));
    }
}
