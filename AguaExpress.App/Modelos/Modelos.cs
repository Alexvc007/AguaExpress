namespace AguaExpress.App.Modelos;

public class SesionDto { public string Token { get; set; } = ""; public int Id { get; set; } public string Nombre { get; set; } = ""; public string Rol { get; set; } = ""; }

public class UsuarioDto
{
    public int Id { get; set; } public string NombreCompleto { get; set; } = ""; public string Correo { get; set; } = "";
    public string Telefono { get; set; } = ""; public string Rol { get; set; } = ""; public bool Activo { get; set; }
    public string Resumen => $"{Rol} · {Correo} · {Telefono}";
    public string EstadoTexto => Activo ? "Activo" : "Desactivado";
}

public class SifonDto
{
    public int Id { get; set; } public string Nombre { get; set; } = ""; public int CapacidadLitros { get; set; }
    public decimal Precio { get; set; } public bool Activo { get; set; } public int StockDisponible { get; set; } public int StockMinimo { get; set; }
    public string PrecioTexto => $"Bs {Precio:N2}";
    public string StockTexto => $"Stock: {StockDisponible} (mín. {StockMinimo})" + (Activo ? "" : " · INACTIVO");
    public bool StockBajo => StockDisponible <= StockMinimo;
}

public class ItemPedidoDto { public string Nombre { get; set; } = ""; public int Cantidad { get; set; } public decimal PrecioUnitario { get; set; } }

public class PedidoDto
{
    public int Id { get; set; } public string ClienteNombre { get; set; } = ""; public string ClienteTelefono { get; set; } = "";
    public int? RepartidorId { get; set; } public string? RepartidorNombre { get; set; }
    public string Direccion { get; set; } = ""; public string? Referencia { get; set; }
    public double Latitud { get; set; } public double Longitud { get; set; }
    public string Estado { get; set; } = ""; public decimal Total { get; set; } public DateTime FechaCreacion { get; set; }
    public string? Notas { get; set; } public string? MotivoNoEntrega { get; set; }
    public List<ItemPedidoDto> Items { get; set; } = new();

    public string Titulo => $"Pedido #{Id}";
    public string TotalTexto => $"Bs {Total:N2}";
    public string ItemsTexto => string.Join("\n", Items.Select(i => $"{i.Cantidad} × {i.Nombre}"));
    public string FechaTexto => FechaCreacion.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
    public string EstadoTexto => Estado switch
    {
        "Pendiente" => "🕐 Pendiente", "Asignado" => "📋 Asignado", "EnCamino" => "🚚 En camino",
        "Entregado" => "✅ Entregado", "NoEntregado" => "⚠️ No entregado", "Cancelado" => "❌ Cancelado", _ => Estado
    };
    public string RepartidorTexto => RepartidorNombre is null ? "Sin asignar" : $"Repartidor: {RepartidorNombre}";
    public bool PuedeCancelar => Estado is "Pendiente" or "Asignado";
    public bool TieneMotivo => !string.IsNullOrWhiteSpace(MotivoNoEntrega);
}

public class PanelDto
{
    public decimal IngresosHoy { get; set; } public int SifonesEntregadosHoy { get; set; } public int PedidosPendientes { get; set; }
    public int PedidosEnCamino { get; set; } public int PedidosEntregadosHoy { get; set; } public List<string> StockBajo { get; set; } = new();
}
