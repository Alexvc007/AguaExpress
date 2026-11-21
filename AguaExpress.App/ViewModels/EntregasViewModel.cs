using System.Collections.ObjectModel;
using AguaExpress.App.Modelos;
using AguaExpress.App.Servicios;
using CommunityToolkit.Mvvm.Input;

namespace AguaExpress.App.ViewModels;

public partial class EntregasViewModel(ApiCliente api) : BaseViewModel(api)
{
    public ObservableCollection<PedidoDto> Pedidos { get; } = new();

    [RelayCommand]
    public Task Cargar() => Ejecutar(async () =>
    {
        var lista = await Api.ObtenerAsync<List<PedidoDto>>("pedidos/mis-entregas");
        Pedidos.Clear();
        foreach (var p in lista) Pedidos.Add(p);
    });

    /// <summary>Botón IR: Deep Link a Google Maps / mapa predeterminado con la ruta trazada.</summary>
    [RelayCommand]
    private Task Ir(PedidoDto p) => MapasServicio.NavegarAsync(p.Latitud, p.Longitud);

    /// <summary>Botón NOTIFICAR: marca el pedido en camino y envía la push al cliente.</summary>
    [RelayCommand]
    private async Task Notificar(PedidoDto p)
    {
        await Ejecutar(() => Api.EnviarAsync(HttpMethod.Post, $"pedidos/{p.Id}/en-camino"));
        await Alerta("Listo", $"Se notificó a {p.ClienteNombre} que su pedido va en camino.");
        await Cargar();
    }

    [RelayCommand]
    private async Task Llamar(PedidoDto p)
    {
        try { PhoneDialer.Default.Open(p.ClienteTelefono); } catch { await Alerta("Aviso", "No se pudo abrir el marcador."); }
    }

    [RelayCommand]
    private async Task Entregado(PedidoDto p)
    {
        if (!await Confirmar("Confirmar entrega", $"¿Entregaste el pedido #{p.Id} a {p.ClienteNombre}?")) return;
        await Ejecutar(() => Api.EnviarAsync(HttpMethod.Post, $"pedidos/{p.Id}/entregar"));
        await Cargar();
    }

    [RelayCommand]
    private async Task NoEntregado(PedidoDto p)
    {
        var motivo = await Shell.Current.DisplayActionSheet("¿Por qué no se entregó?", "Cancelar", null,
            "Cliente ausente", "Dirección incorrecta", "Cliente rechazó el pedido", "Otro motivo");
        if (motivo is null || motivo == "Cancelar") return;
        if (motivo == "Otro motivo")
        {
            motivo = await Shell.Current.DisplayPromptAsync("Motivo", "Describe lo ocurrido:", "Guardar", "Cancelar");
            if (string.IsNullOrWhiteSpace(motivo)) return;
        }
        await Ejecutar(() => Api.EnviarAsync(HttpMethod.Post, $"pedidos/{p.Id}/no-entregado", new { Motivo = motivo }));
        await Cargar();
    }
}
