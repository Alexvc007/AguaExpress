using AguaExpress.Api.Dominio;

namespace AguaExpress.Api.Dtos;

public record RegistroDto(string NombreCompleto, string Correo, string Telefono, string Contrasena);
public record LoginDto(string Correo, string Contrasena);
public record SesionDto(string Token, int Id, string Nombre, string Rol);
public record TokenFcmDto(string Token);

public record NuevoUsuarioDto(string NombreCompleto, string Correo, string Telefono, string Contrasena, RolUsuario Rol);
public record UsuarioDto(int Id, string NombreCompleto, string Correo, string Telefono, RolUsuario Rol, bool Activo);

public record SifonDto(int Id, string Nombre, int CapacidadLitros, decimal Precio, bool Activo, int StockDisponible, int StockMinimo);
public record GuardarSifonDto(string Nombre, int CapacidadLitros, decimal Precio, bool Activo, int? StockInicial);
public record ActualizarStockDto(int StockDisponible, int StockMinimo);

public record ItemNuevoPedidoDto(int TipoSifonId, int Cantidad);
public record NuevoPedidoDto(string Direccion, string? Referencia, double Latitud, double Longitud, string? Notas, List<ItemNuevoPedidoDto> Items);
public record AsignarDto(int VendedorId);
public record NoEntregadoDto(string Motivo);

public record ItemPedidoDto(string Nombre, int Cantidad, decimal PrecioUnitario);
public record PedidoDto(
    int Id, string ClienteNombre, string ClienteTelefono, int? RepartidorId, string? RepartidorNombre,
    string Direccion, string? Referencia, double Latitud, double Longitud, EstadoPedido Estado,
    decimal Total, DateTime FechaCreacion, string? Notas, string? MotivoNoEntrega, List<ItemPedidoDto> Items);

public record PanelDto(decimal IngresosHoy, int SifonesEntregadosHoy, int PedidosPendientes, int PedidosEnCamino,
    int PedidosEntregadosHoy, List<string> StockBajo);
