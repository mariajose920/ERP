// Módulo: Seguridad (auditoría, K4) — Responsable: Cristóbal
using ERP.Application.Interfaces;
using ERP.Domain.Common;
using ERP.Domain.Entities.Seguridad;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ERP.Infrastructure.Data;

/// <summary>
/// Interceptor de SaveChanges que escribe en la tabla Auditorias cada creación, modificación o
/// baja lógica de los documentos y maestros principales. Las líneas de detalle y el kardex no se
/// auditan por separado (quedan cubiertos por su documento cabecera).
/// </summary>
public class AuditoriaInterceptor : SaveChangesInterceptor
{
    private static readonly HashSet<string> EntidadesAuditadas = new()
    {
        "OrdenCompra", "RecepcionCompra", "FacturaCompra",
        "Venta", "PagoVenta", "PagoProveedor",
        "AjusteInventario",
        "AsientoContable", "PeriodoContable", "MatrizIntegracionContable",
        "Producto", "Categoria", "Proveedor", "Cliente", "CuentaContable",
        "Usuario", "Rol"
    };

    private readonly IUsuarioActual _usuarioActual;
    private List<(EntityEntry Entrada, string Accion)> _pendientes = new();
    private bool _guardandoAuditoria;

    public AuditoriaInterceptor(IUsuarioActual usuarioActual)
    {
        _usuarioActual = usuarioActual;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Capturar(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Capturar(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context != null && AgregarRegistros(eventData.Context))
        {
            _guardandoAuditoria = true;
            try { await eventData.Context.SaveChangesAsync(cancellationToken); }
            finally { _guardandoAuditoria = false; }
        }
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (eventData.Context != null && AgregarRegistros(eventData.Context))
        {
            _guardandoAuditoria = true;
            try { eventData.Context.SaveChanges(); }
            finally { _guardandoAuditoria = false; }
        }
        return base.SavedChanges(eventData, result);
    }

    /// <summary>Antes de guardar: registra qué entidades cambian (el Id de las nuevas aún no existe).</summary>
    private void Capturar(DbContext? context)
    {
        if (context == null || _guardandoAuditoria) return;
        _pendientes = context.ChangeTracker.Entries<BaseEntity>()
            .Where(e => EntidadesAuditadas.Contains(e.Entity.GetType().Name))
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(e => (Entrada: (EntityEntry)e, Accion: Accion(e)))
            .ToList();
    }

    /// <summary>Después de guardar: crea los registros de auditoría ya con los Id asignados.</summary>
    private bool AgregarRegistros(DbContext context)
    {
        if (_guardandoAuditoria || _pendientes.Count == 0) return false;

        foreach (var (entrada, accion) in _pendientes)
        {
            var entidad = (BaseEntity)entrada.Entity;
            var tipo = entidad.GetType();
            context.Set<Auditoria>().Add(new Auditoria
            {
                Modulo = tipo.Namespace?.Split('.').LastOrDefault() ?? "General",
                Accion = accion,
                Entidad = tipo.Name,
                EntidadId = entidad.Id.ToString(),
                UsuarioEmail = _usuarioActual.Email,
                IpAddress = _usuarioActual.Ip,
                CreadoPor = _usuarioActual.Email
            });
        }
        _pendientes = new();
        return true;
    }

    private static string Accion(EntityEntry<BaseEntity> e)
    {
        if (e.State == EntityState.Added) return "Creación";
        if (e.State == EntityState.Deleted) return "Eliminación";
        var activo = e.Property(nameof(BaseEntity.Activo));
        if (activo.IsModified && activo.CurrentValue is false) return "Baja lógica";
        return "Modificación";
    }
}
