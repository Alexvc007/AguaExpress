using AguaExpress.Api.Datos;
using AguaExpress.Api.Dominio;
using AguaExpress.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AguaExpress.Api.Controladores;

[ApiController, Route("sifones"), Authorize]
public class SifonesControlador(AguaExpressDb db) : ControladorBase
{
    private IQueryable<SifonDto> Consulta(bool soloActivos) =>
        db.TiposSifon.Where(t => !soloActivos || t.Activo).OrderBy(t => t.CapacidadLitros)
            .Select(t => new SifonDto(t.Id, t.Nombre, t.CapacidadLitros, t.Precio, t.Activo,
                t.Inventario != null ? t.Inventario.StockDisponible : 0,
                t.Inventario != null ? t.Inventario.StockMinimo : 0));

    /// <summary>Clientes ven solo los activos; el administrador ve todos (inventario).</summary>
    [HttpGet]
    public async Task<List<SifonDto>> Listar() =>
        await Consulta(!User.IsInRole("Administrador")).ToListAsync();

    [HttpPost, Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Crear(GuardarSifonDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre) || dto.CapacidadLitros <= 0 || dto.Precio < 0)
            return Error(400, "Datos del sifón inválidos.");
        var t = new TipoSifon
        {
            Nombre = dto.Nombre.Trim(), CapacidadLitros = dto.CapacidadLitros, Precio = dto.Precio, Activo = dto.Activo,
            Inventario = new Inventario { StockDisponible = dto.StockInicial ?? 0 }
        };
        db.TiposSifon.Add(t);
        await db.SaveChangesAsync();
        return Ok(new { t.Id });
    }

    [HttpPut("{id:int}"), Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Actualizar(int id, GuardarSifonDto dto)
    {
        var t = await db.TiposSifon.FindAsync(id);
        if (t is null) return NotFound();
        t.Nombre = dto.Nombre.Trim(); t.CapacidadLitros = dto.CapacidadLitros; t.Precio = dto.Precio; t.Activo = dto.Activo;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id:int}/stock"), Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ActualizarStock(int id, ActualizarStockDto dto)
    {
        if (dto.StockDisponible < 0 || dto.StockMinimo < 0) return Error(400, "Los valores no pueden ser negativos.");
        var inv = await db.Inventario.FirstOrDefaultAsync(i => i.TipoSifonId == id);
        if (inv is null) { inv = new Inventario { TipoSifonId = id }; db.Inventario.Add(inv); }
        inv.StockDisponible = dto.StockDisponible; inv.StockMinimo = dto.StockMinimo;
        inv.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return NoContent();
    }
}
