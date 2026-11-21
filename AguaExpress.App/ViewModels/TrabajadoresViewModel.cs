using System.Collections.ObjectModel;
using AguaExpress.App.Modelos;
using AguaExpress.App.Servicios;
using CommunityToolkit.Mvvm.Input;

namespace AguaExpress.App.ViewModels;

public partial class TrabajadoresViewModel(ApiCliente api) : BaseViewModel(api)
{
    public ObservableCollection<UsuarioDto> Trabajadores { get; } = new();

    [RelayCommand]
    public Task Cargar() => Ejecutar(async () =>
    {
        var lista = await Api.ObtenerAsync<List<UsuarioDto>>("usuarios/trabajadores");
        Trabajadores.Clear();
        foreach (var u in lista) Trabajadores.Add(u);
    });

    [RelayCommand]
    private async Task Nuevo()
    {
        var rol = await Shell.Current.DisplayActionSheet("Tipo de trabajador", "Cancelar", null, "Vendedor", "Administrador");
        if (rol is null || rol == "Cancelar") return;
        var nombre = await Shell.Current.DisplayPromptAsync("Nuevo trabajador", "Nombre completo:", "Siguiente", "Cancelar");
        if (string.IsNullOrWhiteSpace(nombre)) return;
        var correo = await Shell.Current.DisplayPromptAsync("Nuevo trabajador", "Correo:", "Siguiente", "Cancelar", keyboard: Keyboard.Email);
        if (string.IsNullOrWhiteSpace(correo)) return;
        var tel = await Shell.Current.DisplayPromptAsync("Nuevo trabajador", "Teléfono:", "Siguiente", "Cancelar", keyboard: Keyboard.Telephone);
        if (string.IsNullOrWhiteSpace(tel)) return;
        var pass = await Shell.Current.DisplayPromptAsync("Nuevo trabajador", "Contraseña (mín. 6):", "Crear", "Cancelar");
        if (string.IsNullOrWhiteSpace(pass)) return;

        await Ejecutar(() => Api.EnviarAsync(HttpMethod.Post, "usuarios/trabajadores",
            new { NombreCompleto = nombre, Correo = correo, Telefono = tel, Contrasena = pass, Rol = rol }));
        await Cargar();
    }

    [RelayCommand]
    private async Task AlternarActivo(UsuarioDto u)
    {
        var accion = u.Activo ? "desactivar" : "activar";
        if (!await Confirmar("Trabajador", $"¿Deseas {accion} a {u.NombreCompleto}?")) return;
        await Ejecutar(() => Api.EnviarAsync(HttpMethod.Put, $"usuarios/{u.Id}/activo"));
        await Cargar();
    }
}
