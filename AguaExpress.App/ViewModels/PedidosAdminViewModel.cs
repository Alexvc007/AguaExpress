using System.Collections.ObjectModel;
using AguaExpress.App.Modelos;
using AguaExpress.App.Servicios;
using CommunityToolkit.Mvvm.Input;

namespace AguaExpress.App.ViewModels;

public partial class PedidosAdminViewModel(ApiCliente api) : BaseViewModel(api)
{
    public ObservableCollection<PedidoDto> Pedidos { get; } = new();

    [RelayCommand]
    public Task Cargar() => Ejecutar(async () =>
    {
        var lista = await Api.ObtenerAsync<List<PedidoDto>>("pedidos");
        Pedidos.Clear();
        foreach (var p in lista) Pedidos.Add(p);
    });

    [RelayCommand]
    private async Task Opciones(PedidoDto p)
    {
        var acciones = new List<string>();
        if (p.Estado is "Pendiente" or "Asignado") acciones.Add(p.RepartidorId is null ? "Asignar vendedor" : "Reasignar vendedor");
        if (p.PuedeCancelar) acciones.Add("Cancelar pedido");
        if (acciones.Count == 0) { await Alerta(p.Titulo, $"{p.EstadoTexto}\n{p.RepartidorTexto}\n{(p.TieneMotivo ? "Motivo: " + p.MotivoNoEntrega : "")}"); return; }

        var elegida = await Shell.Current.DisplayActionSheet($"{p.Titulo} · {p.ClienteNombre}", "Cerrar", null, acciones.ToArray());
        if (elegida is null || elegida == "Cerrar") return;

        await Ejecutar(async () =>
        {
            if (elegida.Contains("vendedor"))
            {
                var vendedores = (await Api.ObtenerAsync<List<UsuarioDto>>("usuarios/trabajadores"))
                    .Where(u => u.Rol == "Vendedor" && u.Activo).ToList();
                if (vendedores.Count == 0) { await Alerta("Aviso", "Primero registra vendedores en la pestaña Trabajadores."); return; }
                var nombre = await Shell.Current.DisplayActionSheet("Selecciona el vendedor", "Cancelar", null,
                    vendedores.Select(v => v.NombreCompleto).ToArray());
                var v = vendedores.FirstOrDefault(x => x.NombreCompleto == nombre);
                if (v is null) return;
                await Api.EnviarAsync(HttpMethod.Post, $"pedidos/{p.Id}/asignar", new { VendedorId = v.Id });
            }
            else if (await Confirmar("Cancelar pedido", $"¿Cancelar el pedido #{p.Id}?"))
                await Api.EnviarAsync(HttpMethod.Post, $"pedidos/{p.Id}/cancelar");
        });
        await Cargar();
    }
}
