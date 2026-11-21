using AguaExpress.App.Servicios;
using AguaExpress.App.ViewModels;
using AguaExpress.App.Vistas;

namespace AguaExpress.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var b = MauiApp.CreateBuilder();
        b.UseMauiApp<App>();

        b.Services.AddSingleton<SesionServicio>();
        b.Services.AddSingleton<ApiCliente>();
        b.Services.AddSingleton<NotificacionesPush>();

        b.Services.AddTransient<LoginViewModel>(); b.Services.AddTransient<LoginPage>();
        b.Services.AddTransient<RegistroViewModel>(); b.Services.AddTransient<RegistroPage>();
        b.Services.AddTransient<PanelViewModel>(); b.Services.AddTransient<PanelPage>();
        b.Services.AddTransient<PedidosAdminViewModel>(); b.Services.AddTransient<PedidosAdminPage>();
        b.Services.AddTransient<InventarioViewModel>(); b.Services.AddTransient<InventarioPage>();
        b.Services.AddTransient<TrabajadoresViewModel>(); b.Services.AddTransient<TrabajadoresPage>();
        b.Services.AddTransient<EntregasViewModel>(); b.Services.AddTransient<EntregasPage>();
        b.Services.AddTransient<MisPedidosViewModel>(); b.Services.AddTransient<MisPedidosPage>();
        b.Services.AddTransient<NuevoPedidoViewModel>(); b.Services.AddTransient<NuevoPedidoPage>();

        return b.Build();
    }
}