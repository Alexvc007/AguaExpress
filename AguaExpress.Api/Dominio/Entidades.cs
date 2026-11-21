namespace AguaExpress.Api.Dominio;

public enum RolUsuario { Administrador, Vendedor, Cliente }
public enum EstadoPedido { Pendiente, Asignado, EnCamino, Entregado, NoEntregado, Cancelado }

public class Usuario
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = "";
    public string Correo { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string ContrasenaHash { get; set; } = "";
    public RolUsuario Rol { get; set; }
    public bool Activo { get; set; } = true;
    public string? TokenFcm { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

public class TipoSifon
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public int CapacidadLitros { get; set; }
    public decimal Precio { get; set; }
    public bool Activo { get; set; } = true;
    public Inventario? Inventario { get; set; }
}

public class Inventario
{
    public int Id { get; set; }
    public int TipoSifonId { get; set; }
    public TipoSifon TipoSifon { get; set; } = null!;
    public int StockDisponible { get; set; }
    public int StockMinimo { get; set; } = 5;
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
}

public class Pedido
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public Usuario Cliente { get; set; } = null!;
    public int? RepartidorId { get; set; }
    public Usuario? Repartidor { get; set; }
    public string Direccion { get; set; } = "";
    public string? Referencia { get; set; }
    public double Latitud { get; set; }
    public double Longitud { get; set; }
    public EstadoPedido Estado { get; set; } = EstadoPedido.Pendiente;
    public decimal Total { get; set; }
    public string? Notas { get; set; }
    public string? MotivoNoEntrega { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaAsignacion { get; set; }
    public DateTime? FechaEntrega { get; set; }
    public List<DetallePedido> Detalles { get; set; } = new();
}

public class DetallePedido
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public Pedido Pedido { get; set; } = null!;
    public int TipoSifonId { get; set; }
    public TipoSifon TipoSifon { get; set; } = null!;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}
