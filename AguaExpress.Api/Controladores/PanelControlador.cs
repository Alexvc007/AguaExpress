using AguaExpress.Api.Datos;
using AguaExpress.Api.Dominio;
using AguaExpress.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AguaExpress.Api.Controladores;

[ApiController, Route("panel"), Authorize(Roles = "Administrador")]
public class PanelControlador(AguaExpressDb db) : ControladorBase
{
    [HttpGet]
    public async Task<PanelDto> Obtener()
    {
        // "Hoy" según la hora de Bolivia, convertido a UTC para consultar.
        var zona = TimeZoneInfo.FindSystemTimeZoneById("America/La_Paz");
        var hoyLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zona).Date;
        var inicioUtc = TimeZoneInfo.ConvertTimeToUtc(hoyLocal, zona);

        var entregadosHoy = db.Pedidos.Where(p => p.Estado == EstadoPedido.Entregado && p.FechaEntrega >= inicioUtc);

        return new PanelDto(
            IngresosHoy: await entregadosHoy.SumAsync(p => (decimal?)p.Total) ?? 0,
            SifonesEntregadosHoy: await db.DetallesPedido
                .Where(d => d.Pedido.Estado == EstadoPedido.Entregado && d.Pedido.FechaEntrega >= inicioUtc)
                .SumAsync(d => (int?)d.Cantidad) ?? 0,
            PedidosPendientes: await db.Pedidos.CountAsync(p => p.Estado == EstadoPedido.Pendiente),
            PedidosEnCamino: await db.Pedidos.CountAsync(p => p.Estado == EstadoPedido.EnCamino),
            PedidosEntregadosHoy: await entregadosHoy.CountAsync(),
            StockBajo: await db.Inventario.Where(i => i.StockDisponible <= i.StockMinimo && i.TipoSifon.Activo)
                .Select(i => i.TipoSifon.Nombre + ": " + i.StockDisponible + " u.").ToListAsync());
    }
}
