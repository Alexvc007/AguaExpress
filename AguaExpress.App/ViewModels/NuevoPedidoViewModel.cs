using System.Collections.ObjectModel;
using AguaExpress.App.Modelos;
using AguaExpress.App.Servicios;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AguaExpress.App.ViewModels;

public partial class ItemCarrito : ObservableObject
{
    public required SifonDto Sifon { get; init; }
    public Action? Cambio { get; set; }
    [ObservableProperty] private int cantidad;
    partial void OnCantidadChanged(int value) => Cambio?.Invoke();

    [RelayCommand] private void Mas() { if (Cantidad < 50) Cantidad++; }
    [RelayCommand] private void Menos() { if (Cantidad > 0) Cantidad--; }
}

public partial class NuevoPedidoViewModel(ApiCliente api) : BaseViewModel(api)
{
    public ObservableCollection<ItemCarrito> Items { get; } = new();
    [ObservableProperty] private string direccion = "";
    [ObservableProperty] private string referencia = "";
    [ObservableProperty] private string notas = "";
    [ObservableProperty] private string ubicacionTexto = "Sin ubicación. Toca “Usar mi ubicación actual”.";
    [ObservableProperty] private string totalTexto = "Total: Bs 0.00";
    private double _lat, _lng;

    private void Recalcular() =>
        TotalTexto = $"Total: Bs {Items.Sum(i => i.Cantidad * i.Sifon.Precio):N2}";

    [RelayCommand]
    public Task Cargar() => Ejecutar(async () =>
    {
        var lista = await Api.ObtenerAsync<List<SifonDto>>("sifones");
        Items.Clear();
        foreach (var s in lista) Items.Add(new ItemCarrito { Sifon = s, Cambio = Recalcular });
        Recalcular();
    });

    [RelayCommand]
    private Task UsarUbicacion() => Ejecutar(async () =>
    {
        var permiso = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        if (permiso != PermissionStatus.Granted)
        { await Alerta("Ubicación", "Necesitamos tu ubicación para que el repartidor llegue a tu domicilio."); return; }

        var loc = await Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(15)));
        if (loc is null) { await Alerta("Ubicación", "No se pudo obtener tu ubicación. Activa el GPS e intenta de nuevo."); return; }
        _lat = loc.Latitude; _lng = loc.Longitude;
        UbicacionTexto = $"📍 Ubicación guardada ({_lat:F5}, {_lng:F5})";
    });

    [RelayCommand]
    private Task ConfirmarPedido() => Ejecutar(async () =>
    {
        var elegidos = Items.Where(i => i.Cantidad > 0).ToList();
        if (elegidos.Count == 0) { await Alerta("Aviso", "Selecciona al menos un sifón."); return; }
        if (string.IsNullOrWhiteSpace(Direccion)) { await Alerta("Aviso", "Escribe tu dirección de entrega."); return; }
        if (_lat == 0 && _lng == 0) { await Alerta("Aviso", "Toca “Usar mi ubicación actual” para que el repartidor pueda llegar."); return; }

        await Api.EnviarAsync(HttpMethod.Post, "pedidos", new
        {
            Direccion, Referencia, Notas, Latitud = _lat, Longitud = _lng,
            Items = elegidos.Select(i => new { TipoSifonId = i.Sifon.Id, i.Cantidad })
        });
        foreach (var i in Items) i.Cantidad = 0;
        Notas = "";
        await Alerta("¡Pedido enviado!", "Te avisaremos cuando tu repartidor esté en camino.");
        await Shell.Current.GoToAsync("//cliente/MisPedidos");
    });
}
