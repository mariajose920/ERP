using Microsoft.EntityFrameworkCore;
using ERP.Domain.Entities.Seguridad;
using ERP.Domain.Entities.Maestros;
using ERP.Domain.Entities.Compras;
using ERP.Domain.Entities.Ventas;
using ERP.Domain.Entities.Inventario;
using ERP.Domain.Entities.Contabilidad;

namespace ERP.Infrastructure.Data;

public class ErpDbContext : DbContext
{
    public ErpDbContext(DbContextOptions<ErpDbContext> options) : base(options)
    {
    }

    // Seguridad & RBAC
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Permiso> Permisos => Set<Permiso>();
    public DbSet<RolPermiso> RolPermisos => Set<RolPermiso>();
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();

    // Maestros
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<CuentaContable> CuentasContables => Set<CuentaContable>();

    // Compras
    public DbSet<OrdenCompra> OrdenesCompra => Set<OrdenCompra>();
    public DbSet<DetalleOrdenCompra> DetallesOrdenCompra => Set<DetalleOrdenCompra>();
    public DbSet<RecepcionCompra> RecepcionesCompra => Set<RecepcionCompra>();
    public DbSet<DetalleRecepcionCompra> DetallesRecepcionCompra => Set<DetalleRecepcionCompra>();
    public DbSet<FacturaCompra> FacturasCompra => Set<FacturaCompra>();
    public DbSet<DetalleFacturaCompra> DetallesFacturaCompra => Set<DetalleFacturaCompra>();

    // Ventas
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();
    public DbSet<PagoVenta> PagosVenta => Set<PagoVenta>();

    // Inventario
    public DbSet<MovimientoKardex> MovimientosKardex => Set<MovimientoKardex>();
    public DbSet<AjusteInventario> AjustesInventario => Set<AjusteInventario>();

    // Contabilidad
    public DbSet<AsientoContable> AsientosContables => Set<AsientoContable>();
    public DbSet<DetalleAsientoContable> DetallesAsientoContable => Set<DetalleAsientoContable>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // RolPermiso composite primary key
        modelBuilder.Entity<RolPermiso>()
            .HasKey(rp => new { rp.RolId, rp.PermisoId });

        modelBuilder.Entity<RolPermiso>()
            .HasOne(rp => rp.Rol)
            .WithMany(r => r.RolPermisos)
            .HasForeignKey(rp => rp.RolId);

        modelBuilder.Entity<RolPermiso>()
            .HasOne(rp => rp.Permiso)
            .WithMany(p => p.RolPermisos)
            .HasForeignKey(rp => rp.PermisoId);

        // Precision configurations for monetary & quantitative properties
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var properties = entityType.ClrType.GetProperties()
                .Where(p => p.PropertyType == typeof(decimal) || p.PropertyType == typeof(decimal?));

            foreach (var property in properties)
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(property.Name)
                    .HasPrecision(18, 4);
            }
        }

        // Indices
        modelBuilder.Entity<Usuario>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<Producto>().HasIndex(p => p.Sku).IsUnique();
        modelBuilder.Entity<Proveedor>().HasIndex(pr => pr.Rut).IsUnique();
        modelBuilder.Entity<Cliente>().HasIndex(c => c.Rut).IsUnique();
        modelBuilder.Entity<CuentaContable>().HasIndex(cc => cc.Codigo).IsUnique();

        // 1. Rename tables for intuitiveness
        modelBuilder.Entity<FacturaCompra>().ToTable("ComprasProveedores");
        modelBuilder.Entity<DetalleFacturaCompra>().ToTable("LineasCompraProveedor");
        modelBuilder.Entity<OrdenCompra>().ToTable("OrdenesCompraProveedor");
        modelBuilder.Entity<RecepcionCompra>().ToTable("RecepcionesMercaderia");
        modelBuilder.Entity<Venta>().ToTable("VentasClientes");
        modelBuilder.Entity<PagoVenta>().ToTable("CobrosClientes");
        modelBuilder.Entity<MovimientoKardex>().ToTable("KardexInventario");

        // 2. Fix ON DELETE CASCADE for Productos
        modelBuilder.Entity<DetalleFacturaCompra>()
            .HasOne(d => d.Producto)
            .WithMany()
            .HasForeignKey(d => d.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DetalleOrdenCompra>()
            .HasOne(d => d.Producto)
            .WithMany()
            .HasForeignKey(d => d.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DetalleRecepcionCompra>()
            .HasOne(d => d.Producto)
            .WithMany()
            .HasForeignKey(d => d.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DetalleVenta>()
            .HasOne(d => d.Producto)
            .WithMany()
            .HasForeignKey(d => d.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MovimientoKardex>()
            .HasOne(m => m.Producto)
            .WithMany()
            .HasForeignKey(m => m.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AjusteInventario>()
            .HasOne(a => a.Producto)
            .WithMany()
            .HasForeignKey(a => a.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        // 3. Enforce Foreign Keys to AsientosContables
        modelBuilder.Entity<FacturaCompra>()
            .HasOne<AsientoContable>()
            .WithMany()
            .HasForeignKey(f => f.AsientoContableId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Venta>()
            .HasOne<AsientoContable>()
            .WithMany()
            .HasForeignKey(v => v.AsientoVentaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Venta>()
            .HasOne<AsientoContable>()
            .WithMany()
            .HasForeignKey(v => v.AsientoCostoVentaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PagoVenta>()
            .HasOne<AsientoContable>()
            .WithMany()
            .HasForeignKey(p => p.AsientoPagoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AjusteInventario>()
            .HasOne<AsientoContable>()
            .WithMany()
            .HasForeignKey(a => a.AsientoContableId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
