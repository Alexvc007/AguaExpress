using AguaExpress.Api.Dominio;
using Microsoft.EntityFrameworkCore;

namespace AguaExpress.Api.Datos;

public class AguaExpressDb(DbContextOptions<AguaExpressDb> opciones) : DbContext(opciones)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<TipoSifon> TiposSifon => Set<TipoSifon>();
    public DbSet<Inventario> Inventario => Set<Inventario>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<DetallePedido> DetallesPedido => Set<DetallePedido>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Usuario>(e =>
        {
            e.ToTable("usuarios");
            e.HasIndex(u => u.Correo).IsUnique();
            e.Property(u => u.Rol).HasConversion<string>().HasMaxLength(20);
            e.Property(u => u.Correo).HasMaxLength(150);
        });
        mb.Entity<TipoSifon>(e =>
        {
            e.ToTable("tipos_sifon");
            e.Property(t => t.Precio).HasPrecision(10, 2);
        });
        mb.Entity<Inventario>(e =>
        {
            e.ToTable("inventario");
            e.HasIndex(i => i.TipoSifonId).IsUnique();
            e.HasOne(i => i.TipoSifon).WithOne(t => t.Inventario).HasForeignKey<Inventario>(i => i.TipoSifonId);
        });
        mb.Entity<Pedido>(e =>
        {
            e.ToTable("pedidos");
            e.Property(p => p.Estado).HasConversion<string>().HasMaxLength(20);
            e.Property(p => p.Total).HasPrecision(10, 2);
            e.HasOne(p => p.Cliente).WithMany().HasForeignKey(p => p.ClienteId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.Repartidor).WithMany().HasForeignKey(p => p.RepartidorId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(p => new { p.Estado, p.FechaCreacion });
        });
        mb.Entity<DetallePedido>(e =>
        {
            e.ToTable("detalles_pedido");
            e.Property(d => d.PrecioUnitario).HasPrecision(10, 2);
        });
    }
}

public static class Semilla
{
    public static async Task EjecutarAsync(AguaExpressDb db, IConfiguration cfg)
    {
        if (!await db.Usuarios.AnyAsync(u => u.Rol == RolUsuario.Administrador))
        {
            db.Usuarios.Add(new Usuario
            {
                NombreCompleto = cfg["AdministradorInicial:Nombre"] ?? "Administrador",
                Correo = (cfg["AdministradorInicial:Correo"] ?? "admin@aguaexpress.com").ToLowerInvariant(),
                Telefono = "00000000",
                ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(cfg["AdministradorInicial:Contrasena"] ?? "Admin123!"),
                Rol = RolUsuario.Administrador
            });
        }
        if (!await db.TiposSifon.AnyAsync())
        {
            foreach (var (litros, precio) in new[] { (1, 4m), (3, 8m), (5, 12m), (10, 20m), (15, 28m) })
                db.TiposSifon.Add(new TipoSifon
                {
                    Nombre = $"Sifón {litros} L",
                    CapacidadLitros = litros,
                    Precio = precio,
                    Inventario = new Inventario { StockDisponible = 50, StockMinimo = 10 }
                });
        }
        await db.SaveChangesAsync();
    }
}
