// Módulo: Transversal (persistencia) — Responsable: Cristóbal
using Microsoft.EntityFrameworkCore;
using ERP.Domain.Entities.Seguridad;
using ERP.Domain.Entities.Maestros;
using ERP.Domain.Entities.Compras;
using ERP.Domain.Entities.Ventas;
using ERP.Domain.Entities.Inventario;
using ERP.Domain.Entities.Contabilidad;

namespace ERP.Infrastructure.Data;

/// <summary>Contexto EF Core del ERP (SQL Server). Las tablas de Compras/Ventas/Kardex tienen nombres propios (ver OnModelCreating).</summary>
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
    public DbSet<PagoProveedor> PagosProveedor => Set<PagoProveedor>();

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
    public DbSet<PeriodoContable> PeriodosContables => Set<PeriodoContable>();
    public DbSet<MatrizIntegracionContable> MatrizIntegracionContable => Set<MatrizIntegracionContable>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Clave primaria compuesta de RolPermiso
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

        // Precisión (18,4) para todos los montos y cantidades decimales
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

        // Índices únicos
        modelBuilder.Entity<Usuario>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<Producto>().HasIndex(p => p.Sku).IsUnique();
        modelBuilder.Entity<Proveedor>().HasIndex(pr => pr.Rut).IsUnique();
        modelBuilder.Entity<Cliente>().HasIndex(c => c.Rut).IsUnique();
        modelBuilder.Entity<CuentaContable>().HasIndex(cc => cc.Codigo).IsUnique();

        // 1. Nombres de tabla más descriptivos
        modelBuilder.Entity<FacturaCompra>().ToTable("ComprasProveedores");
        modelBuilder.Entity<DetalleFacturaCompra>().ToTable("LineasCompraProveedor");
        modelBuilder.Entity<OrdenCompra>().ToTable("OrdenesCompraProveedor");
        modelBuilder.Entity<RecepcionCompra>().ToTable("RecepcionesMercaderia");
        modelBuilder.Entity<Venta>().ToTable("VentasClientes");
        modelBuilder.Entity<PagoVenta>().ToTable("CobrosClientes");
        modelBuilder.Entity<MovimientoKardex>().ToTable("KardexInventario");

        // 2. Evitar borrado en cascada desde Productos (DeleteBehavior.Restrict)
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

        // 3. Claves foráneas explícitas hacia AsientosContables
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

        // 4. Pagos a proveedores (espejo de CobrosClientes)
        modelBuilder.Entity<PagoProveedor>().ToTable("PagosProveedores");
        modelBuilder.Entity<PagoProveedor>()
            .HasOne(p => p.FacturaCompra)
            .WithMany(f => f.Pagos)
            .HasForeignKey(p => p.CompraProveedorId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<PagoProveedor>()
            .HasOne<AsientoContable>()
            .WithMany()
            .HasForeignKey(p => p.AsientoPagoId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<PagoProveedor>().HasIndex(p => p.CompraProveedorId);

        // 5. Reversa de asientos: FK auto-referencia (ON DELETE NO ACTION) + índice
        modelBuilder.Entity<AsientoContable>()
            .HasOne<AsientoContable>()
            .WithMany()
            .HasForeignKey(a => a.AsientoRevertidoId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<AsientoContable>().HasIndex(a => a.AsientoRevertidoId);

        // 6. Períodos contables: uno por (Año, Mes)
        modelBuilder.Entity<PeriodoContable>().ToTable("PeriodosContables");
        modelBuilder.Entity<PeriodoContable>().HasIndex(p => new { p.Anio, p.Mes }).IsUnique();

        // 7. Matriz de integración contable (módulo + operación -> cuentas Debe/Haber)
        modelBuilder.Entity<MatrizIntegracionContable>().ToTable("MatrizIntegracionContable");
        modelBuilder.Entity<MatrizIntegracionContable>().Property(m => m.ModuloOrigen).HasMaxLength(50);
        modelBuilder.Entity<MatrizIntegracionContable>().Property(m => m.TipoOperacion).HasMaxLength(100);
        modelBuilder.Entity<MatrizIntegracionContable>().Property(m => m.Descripcion).HasMaxLength(300);
        modelBuilder.Entity<MatrizIntegracionContable>().HasIndex(m => new { m.ModuloOrigen, m.TipoOperacion }).IsUnique();
        modelBuilder.Entity<MatrizIntegracionContable>()
            .HasOne(m => m.CuentaDebe).WithMany().HasForeignKey(m => m.CuentaDebeId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<MatrizIntegracionContable>()
            .HasOne(m => m.CuentaHaber).WithMany().HasForeignKey(m => m.CuentaHaberId).OnDelete(DeleteBehavior.Restrict);

        // 8. Motivo de anulación y comisión (override de la precisión general decimal(18,4))
        modelBuilder.Entity<Usuario>().Property(u => u.PorcentajeComision).HasPrecision(5, 2);
        modelBuilder.Entity<Venta>().Property(v => v.MotivoAnulacion).HasMaxLength(300);
        modelBuilder.Entity<OrdenCompra>().Property(o => o.MotivoAnulacion).HasMaxLength(300);
        modelBuilder.Entity<FacturaCompra>().Property(f => f.MotivoAnulacion).HasMaxLength(300);
    }
}
