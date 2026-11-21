using System.Collections.ObjectModel;
using AguaExpress.App.Modelos;
using AguaExpress.App.Servicios;
using CommunityToolkit.Mvvm.Input;

namespace AguaExpress.App.ViewModels;

public partial class MisPedidosViewModel(ApiCliente api) : BaseViewModel(api)
{
    public ObservableCollection<PedidoDto> Pedidos { get; } = new();

    [RelayCommand]
    public Task Cargar() => Ejecutar(async () =>
    {
        var lista = await Api.ObtenerAsync<List<PedidoDto>>("pedidos/mios");
        Pedidos.Clear();
        foreach (var p in lista) Pedidos.Add(p);
    });

    [RelayCommand]
    private async Task Cancelar(PedidoDto p)
    {
        if (!await Confirmar("Cancelar pedido", $"¿Seguro que deseas cancelar el pedido #{p.Id}?")) return;
        await Ejecutar(() => Api.EnviarAsync(HttpMethod.Post, $"pedidos/{p.Id}/cancelar"));
        await Cargar();
    }
}
