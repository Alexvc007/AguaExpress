using System.Collections.ObjectModel;
using AguaExpress.App.Modelos;
using AguaExpress.App.Servicios;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AguaExpress.App.ViewModels;

public partial class PanelViewModel(ApiCliente api) : BaseViewModel(api)
{
    [ObservableProperty] private string ingresosHoy = "Bs 0.00";
    [ObservableProperty] private int sifonesEntregados;
    [ObservableProperty] private int pendientes;
    [ObservableProperty] private int enCamino;
    [ObservableProperty] private int entregadosHoy;
    public ObservableCollection<string> StockBajo { get; } = new();

    [RelayCommand]
    public Task Cargar() => Ejecutar(async () =>
    {
        var p = await Api.ObtenerAsync<PanelDto>("panel");
        IngresosHoy = $"Bs {p.IngresosHoy:N2}";
        SifonesEntregados = p.SifonesEntregadosHoy;
        Pendientes = p.PedidosPendientes; EnCamino = p.PedidosEnCamino; EntregadosHoy = p.PedidosEntregadosHoy;
        StockBajo.Clear();
        foreach (var s in p.StockBajo) StockBajo.Add(s);
    });
}
