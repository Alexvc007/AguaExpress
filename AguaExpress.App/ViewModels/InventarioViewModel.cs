using System.Collections.ObjectModel;
using System.Globalization;
using AguaExpress.App.Modelos;
using AguaExpress.App.Servicios;
using CommunityToolkit.Mvvm.Input;

namespace AguaExpress.App.ViewModels;

public partial class InventarioViewModel(ApiCliente api) : BaseViewModel(api)
{
    public ObservableCollection<SifonDto> Sifones { get; } = new();

    [RelayCommand]
    public Task Cargar() => Ejecutar(async () =>
    {
        var lista = await Api.ObtenerAsync<List<SifonDto>>("sifones");
        Sifones.Clear();
        foreach (var s in lista) Sifones.Add(s);
    });

    private static bool Num(string? t, out decimal v) =>
        decimal.TryParse(t?.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out v);

    [RelayCommand]
    private async Task Nuevo()
    {
        var litros = await Shell.Current.DisplayPromptAsync("Nuevo sifón", "Capacidad en litros:", "Siguiente", "Cancelar", "10", keyboard: Keyboard.Numeric);
        if (!Num(litros, out var l) || l <= 0) return;
        var precio = await Shell.Current.DisplayPromptAsync("Nuevo sifón", "Precio (Bs):", "Siguiente", "Cancelar", "0.00", keyboard: Keyboard.Numeric);
        if (!Num(precio, out var p)) return;
        var stock = await Shell.Current.DisplayPromptAsync("Nuevo sifón", "Stock inicial:", "Guardar", "Cancelar", "0", keyboard: Keyboard.Numeric);
        if (!Num(stock, out var st)) return;

        await Ejecutar(() => Api.EnviarAsync(HttpMethod.Post, "sifones",
            new { Nombre = $"Sifón {(int)l} L", CapacidadLitros = (int)l, Precio = p, Activo = true, StockInicial = (int)st }));
        await Cargar();
    }

    [RelayCommand]
    private async Task Editar(SifonDto s)
    {
        var op = await Shell.Current.DisplayActionSheet(s.Nombre, "Cerrar", null,
            "Actualizar stock", "Cambiar precio", s.Activo ? "Desactivar" : "Activar");
        if (op is null || op == "Cerrar") return;

        if (op == "Actualizar stock")
        {
            var t = await Shell.Current.DisplayPromptAsync(s.Nombre, "Nuevo stock disponible:", "Siguiente", "Cancelar", s.StockDisponible.ToString(), keyboard: Keyboard.Numeric);
            if (!Num(t, out var stock)) return;
            var m = await Shell.Current.DisplayPromptAsync(s.Nombre, "Stock mínimo (alerta):", "Guardar", "Cancelar", s.StockMinimo.ToString(), keyboard: Keyboard.Numeric);
            if (!Num(m, out var min)) return;
            await Ejecutar(() => Api.EnviarAsync(HttpMethod.Put, $"sifones/{s.Id}/stock", new { StockDisponible = (int)stock, StockMinimo = (int)min }));
        }
        else
        {
            decimal precio = s.Precio;
            if (op == "Cambiar precio")
            {
                var t = await Shell.Current.DisplayPromptAsync(s.Nombre, "Nuevo precio (Bs):", "Guardar", "Cancelar", s.Precio.ToString("0.00", CultureInfo.InvariantCulture), keyboard: Keyboard.Numeric);
                if (!Num(t, out precio)) return;
            }
            var activo = op == "Cambiar precio" ? s.Activo : !s.Activo;
            await Ejecutar(() => Api.EnviarAsync(HttpMethod.Put, $"sifones/{s.Id}",
                new { s.Nombre, s.CapacidadLitros, Precio = precio, Activo = activo, StockInicial = (int?)null }));
        }
        await Cargar();
    }
}
