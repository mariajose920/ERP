// Módulo: Seguridad (auditoría, K4) — Responsable: Cristóbal
using ERP.Application.Interfaces;
using ERP.Domain.Common;
using ERP.Domain.Entities.Seguridad;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Globalization;
using System.Text.Json;

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
    private List<(EntityEntry Entrada, string Accion, string Detalles)> _pendientes = new();
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
            .ToList()
            .Select(e => (Entrada: (EntityEntry)e, Accion: Accion(e), Detalles: CapturarDetalles(e)))
            .ToList();
    }

    /// <summary>Después de guardar: crea los registros de auditoría ya con los Id asignados.</summary>
    private bool AgregarRegistros(DbContext context)
    {
        if (_guardandoAuditoria || _pendientes.Count == 0) return false;

        foreach (var (entrada, accion, detalles) in _pendientes)
        {
            var entidad = (BaseEntity)entrada.Entity;
            var tipo = entidad.GetType();
            context.Set<Auditoria>().Add(new Auditoria
            {
                Modulo = tipo.Namespace?.Split('.').LastOrDefault() ?? "General",
                Accion = accion,
                Entidad = tipo.Name,
                EntidadId = entidad.Id.ToString(),
                Detalles = detalles,
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

    // Capturar antes de guardar: EF reemplaza los valores originales tras SaveChanges.
    private static string CapturarDetalles(EntityEntry<BaseEntity> entrada)
    {
        var entidad = entrada.Entity;
        var nombre = new[] { "NombreCompleto", "Nombre", "RazonSocial", "NumeroFactura", "NumeroOrden", "NumeroAsiento", "Codigo", "Email" }
            .Select(n => entrada.Metadata.FindProperty(n) == null ? null : entrada.Property(n).CurrentValue?.ToString())
            .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
        var registro = entidad is Usuario usuario
            ? $"{usuario.NombreCompleto} ({usuario.Email})"
            : nombre ?? entidad.GetType().Name;
        var ignorados = new HashSet<string> { "Id", "FechaCreacion", "FechaModificacion", "CreadoPor", "ModificadoPor" };
        var cambios = new List<object>();
        foreach (var p in entrada.Properties)
        {
            if (ignorados.Contains(p.Metadata.Name)) continue;
            if (entrada.State == EntityState.Modified && (!p.IsModified || Equals(p.OriginalValue, p.CurrentValue))) continue;
            // Nunca persistir contraseñas, hashes, tokens ni secretos en los detalles.
            var sensible = new[] { "password", "contrasena", "contraseña", "secret", "token" }
                .Any(s => p.Metadata.Name.Contains(s, StringComparison.OrdinalIgnoreCase));
            if (sensible)
            {
                if (entrada.State == EntityState.Modified)
                    cambios.Add(new { campo = "Contraseña", anterior = (string?)null, actual = "Cambiada (contenido protegido)" });
                continue;
            }
            cambios.Add(new
            {
                campo = Etiqueta(p.Metadata.Name),
                anterior = entrada.State == EntityState.Added ? null : Valor(p.OriginalValue, p.Metadata.Name, entrada.Context),
                actual = entrada.State == EntityState.Deleted ? null : Valor(p.CurrentValue, p.Metadata.Name, entrada.Context)
            });
        }
        return JsonSerializer.Serialize(new { version = 1, registro, cambios });
    }

    private static string Etiqueta(string nombre) => nombre switch
    {
        "NombreCompleto" => "Nombre completo",
        "RolId" => "Rol",
        "PorcentajeComision" => "Comisión de ventas",
        "Email" => "Email / usuario",
        "Rut" => "RUT",
        "Activo" => "Estado",
        "UltimoAcceso" => "Último acceso",
        _ => nombre
    };

    private static string? Valor(object? valor, string campo, DbContext context)
    {
        if (campo == "PorcentajeComision")
            return valor == null ? "2 % (por defecto)" : $"{Convert.ToDecimal(valor).ToString("0.##", CultureInfo.GetCultureInfo("es-CL"))} %";
        if (valor == null) return null;
        if (campo == "RolId" && valor is int id)
            return context.Set<Rol>().Find(id)?.Nombre ?? $"Rol #{id}";
        if (valor is bool activo) return campo == "Activo" ? (activo ? "Activo" : "Inactivo") : (activo ? "Sí" : "No");
        if (valor is DateTime fecha) return fecha.ToString("O", CultureInfo.InvariantCulture);
        return Convert.ToString(valor, CultureInfo.GetCultureInfo("es-CL"));
    }
}
