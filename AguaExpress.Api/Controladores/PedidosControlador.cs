using AguaExpress.Api.Datos;
using AguaExpress.Api.Dominio;
using AguaExpress.Api.Dtos;
using AguaExpress.Api.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AguaExpress.Api.Controladores;

[ApiController, Route("pedidos"), Authorize]
public class PedidosControlador(AguaExpressDb db, ServicioNotificaciones push) : ControladorBase
{
    private static IQueryable<PedidoDto> Proyectar(IQueryable<Pedido> q) =>
        q.OrderByDescending(p => p.FechaCreacion).Select(p => new PedidoDto(
            p.Id, p.Cliente.NombreCompleto, p.Cliente.Telefono, p.RepartidorId,
            p.Repartidor != null ? p.Repartidor.NombreCompleto : null,
            p.Direccion, p.Referencia, p.Latitud, p.Longitud, p.Estado, p.Total, p.FechaCreacion,
            p.Notas, p.MotivoNoEntrega,
            p.Detalles.Select(d => new ItemPedidoDto(d.TipoSifon.Nombre, d.Cantidad, d.PrecioUnitario)).ToList()));

    private Task<Pedido?> Cargar(int id) =>
        db.Pedidos.Include(p => p.Cliente).Include(p => p.Detalles).FirstOrDefaultAsync(p => p.Id == id);

    // ---------- Consultas ----------
    [HttpGet, Authorize(Roles = "Administrador")]
    public async Task<List<PedidoDto>> Listar(EstadoPedido? estado) =>
        await Proyectar(db.Pedidos.Where(p => estado == null || p.Estado == estado)).Take(300).ToListAsync();

    [HttpGet("mios"), Authorize(Roles = "Cliente")]
    public async Task<List<PedidoDto>> Mios() =>
        await Proyectar(db.Pedidos.Where(p => p.ClienteId == UsuarioId)).Take(100).ToListAsync();

    [HttpGet("mis-entregas"), Authorize(Roles = "Vendedor")]
    public async Task<List<PedidoDto>> MisEntregas() =>
        await Proyectar(db.Pedidos.Where(p => p.RepartidorId == UsuarioId &&
            (p.Estado == EstadoPedido.Asignado || p.Estado == EstadoPedido.EnCamino))).ToListAsync();

    // ---------- Cliente ----------
    [HttpPost, Authorize(Roles = "Cliente")]
    public async Task<IActionResult> Crear(NuevoPedidoDto dto)
    {
        var items = dto.Items.Where(i => i.Cantidad > 0).ToList();
        if (items.Count == 0) return Error(400, "Agrega al menos un sifón al pedido.");
        if (string.IsNullOrWhiteSpace(dto.Direccion)) return Error(400, "Indica la dirección de entrega.");
        if (dto.Latitud == 0 && dto.Longitud == 0) return Error(400, "Debes indicar tu ubicación para la entrega.");

        var ids = items.Select(i => i.TipoSifonId).ToList();
        var tipos = await db.TiposSifon.Where(t => ids.Contains(t.Id) && t.Activo).ToDictionaryAsync(t => t.Id);
        if (tipos.Count != ids.Distinct().Count()) return Error(400, "Alguno de los sifones ya no está disponible.");

        var pedido = new Pedido
        {
            ClienteId = UsuarioId, Direccion = dto.Direccion.Trim(), Referencia = dto.Referencia?.Trim(),
            Latitud = dto.Latitud, Longitud = dto.Longitud, Notas = dto.Notas?.Trim(),
            Detalles = items.Select(i => new DetallePedido
            {
                TipoSifonId = i.TipoSifonId, Cantidad = i.Cantidad, PrecioUnitario = tipos[i.TipoSifonId].Precio
            }).ToList()
        };
        pedido.Total = pedido.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);
        db.Pedidos.Add(pedido);
        await db.SaveChangesAsync();

        var admins = await db.Usuarios.Where(u => u.Rol == RolUsuario.Administrador && u.Activo)
            .Select(u => u.TokenFcm).ToListAsync();
        await push.EnviarAVariosAsync(admins, "Nuevo pedido", $"Pedido #{pedido.Id} por Bs {pedido.Total:N2}");
        return Ok(new { pedido.Id });
    }

    [HttpPost("{id:int}/cancelar"), Authorize(Roles = "Cliente,Administrador")]
    public async Task<IActionResult> Cancelar(int id)
    {
        var p = await Cargar(id);
        if (p is null) return NotFound();
        if (User.IsInRole("Cliente") && p.ClienteId != UsuarioId) return Forbid();
        if (p.Estado is not (EstadoPedido.Pendiente or EstadoPedido.Asignado))
            return Error(409, "El pedido ya no se puede cancelar porque está en camino o finalizado.");

        var repartidorToken = p.RepartidorId is null ? null :
            await db.Usuarios.Where(u => u.Id == p.RepartidorId).Select(u => u.TokenFcm).FirstOrDefaultAsync();
        p.Estado = EstadoPedido.Cancelado;
        await db.SaveChangesAsync();
        await push.EnviarAsync(repartidorToken, "Pedido cancelado", $"El pedido #{p.Id} fue cancelado.");
        if (!User.IsInRole("Cliente"))
            await push.EnviarAsync(p.Cliente.TokenFcm, "Pedido cancelado", $"Tu pedido #{p.Id} fue cancelado.");
        return NoContent();
    }

    // ---------- Administrador ----------
    [HttpPost("{id:int}/asignar"), Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Asignar(int id, AsignarDto dto)
    {
        var p = await Cargar(id);
        if (p is null) return NotFound();
        if (p.Estado is not (EstadoPedido.Pendiente or EstadoPedido.Asignado))
            return Error(409, "Solo se pueden asignar pedidos pendientes o asignados.");
        var v = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == dto.VendedorId && u.Rol == RolUsuario.Vendedor && u.Activo);
        if (v is null) return Error(400, "El vendedor no existe o está desactivado.");

        p.RepartidorId = v.Id; p.Estado = EstadoPedido.Asignado; p.FechaAsignacion = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await push.EnviarAsync(v.TokenFcm, "Nueva entrega asignada", $"Pedido #{p.Id}: {p.Direccion}");
        return NoContent();
    }

    // ---------- Vendedor ----------
    private async Task<(Pedido? pedido, IActionResult? error)> PedidoDelVendedor(int id, params EstadoPedido[] permitidos)
    {
        var p = await Cargar(id);
        if (p is null) return (null, NotFound());
        if (p.RepartidorId != UsuarioId) return (null, Forbid());
        if (!permitidos.Contains(p.Estado)) return (null, Error(409, $"El pedido está en estado {p.Estado}."));
        return (p, null);
    }

    /// <summary>Botón "Notificar al cliente": marca el pedido en camino y avisa al cliente.</summary>
    [HttpPost("{id:int}/en-camino"), Authorize(Roles = "Vendedor")]
    public async Task<IActionResult> EnCamino(int id)
    {
        var (p, err) = await PedidoDelVendedor(id, EstadoPedido.Asignado, EstadoPedido.EnCamino);
        if (err is not null) return err;
        p!.Estado = EstadoPedido.EnCamino;
        await db.SaveChangesAsync();
        await push.EnviarAsync(p.Cliente.TokenFcm, "¡Tu pedido va en camino! 🚚",
            "Tu repartidor de AguaExpress está en camino a tu domicilio.", new() { ["pedidoId"] = p.Id.ToString() });
        return NoContent();
    }

    [HttpPost("{id:int}/entregar"), Authorize(Roles = "Vendedor")]
    public async Task<IActionResult> Entregar(int id)
    {
        var (p, err) = await PedidoDelVendedor(id, EstadoPedido.Asignado, EstadoPedido.EnCamino);
        if (err is not null) return err;

        p!.Estado = EstadoPedido.Entregado; p.FechaEntrega = DateTime.UtcNow;
        foreach (var d in p.Detalles)
        {
            var inv = await db.Inventario.FirstOrDefaultAsync(i => i.TipoSifonId == d.TipoSifonId);
            if (inv is null) continue;
            inv.StockDisponible = Math.Max(0, inv.StockDisponible - d.Cantidad);
            inv.FechaActualizacion = DateTime.UtcNow;
        }
        await db.SaveChangesAsync();
        await push.EnviarAsync(p.Cliente.TokenFcm, "Pedido entregado ✅", $"Tu pedido #{p.Id} fue entregado. ¡Gracias!");
        return NoContent();
    }

    [HttpPost("{id:int}/no-entregado"), Authorize(Roles = "Vendedor")]
    public async Task<IActionResult> NoEntregado(int id, NoEntregadoDto dto)
    {
        var (p, err) = await PedidoDelVendedor(id, EstadoPedido.Asignado, EstadoPedido.EnCamino);
        if (err is not null) return err;
        p!.Estado = EstadoPedido.NoEntregado;
        p.MotivoNoEntrega = string.IsNullOrWhiteSpace(dto.Motivo) ? "Cliente ausente" : dto.Motivo.Trim();
        await db.SaveChangesAsync();

        await push.EnviarAsync(p.Cliente.TokenFcm, "No pudimos entregar tu pedido",
            $"Pedido #{p.Id}: {p.MotivoNoEntrega}. Puedes volver a pedir desde la app.");
        var admins = await db.Usuarios.Where(u => u.Rol == RolUsuario.Administrador && u.Activo).Select(u => u.TokenFcm).ToListAsync();
        await push.EnviarAVariosAsync(admins, "Entrega fallida", $"Pedido #{p.Id}: {p.MotivoNoEntrega}");
        return NoContent();
    }
}
