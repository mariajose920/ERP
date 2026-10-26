// Módulo: Contabilidad — Responsable: Cristóbal
using ERP.Domain.Common;
using ERP.Domain.Entities.Maestros;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities.Contabilidad;

public class AsientoContable : BaseEntity
{
    public string NumeroAsiento { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string Glosa { get; set; } = string.Empty;
    public string ModuloOrigen { get; set; } = "Contabilidad"; // "Compras", "Ventas", "Inventario", "Contabilidad"
    public string? DocumentoOrigenTipo { get; set; } // "FacturaCompra", "FacturaVenta", "AjusteInventario", "Pago"
    public int? DocumentoOrigenId { get; set; }
    public decimal TotalDebe { get; set; }
    public decimal TotalHaber { get; set; }

    /// <summary>Si este asiento es una reversa, id del asiento original que revierte (FK auto-referencia, NO ACTION).</summary>
    public int? AsientoRevertidoId { get; set; }

    public bool Cuadrado => TotalDebe == TotalHaber;

    public ICollection<DetalleAsientoContable> Detalles { get; set; } = new List<DetalleAsientoContable>();
}

public class DetalleAsientoContable : BaseEntity
{
    public int AsientoContableId { get; set; }
    public AsientoContable? AsientoContable { get; set; }
    public int CuentaContableId { get; set; }
    public CuentaContable? CuentaContable { get; set; }
    public decimal Debe { get; set; }
    public decimal Haber { get; set; }
    public string? GlosaLinea { get; set; }
    public string? Referencia { get; set; }
}

/// <summary>Período contable mensual. Un período Cerrado no admite nuevos asientos.</summary>
public class PeriodoContable : BaseEntity
{
    public int Anio { get; set; }
    public int Mes { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public EstadoPeriodoContable Estado { get; set; } = EstadoPeriodoContable.Abierto;
    public DateTime? FechaCierre { get; set; }
    public string? CerradoPor { get; set; }
}

/// <summary>
/// Matriz de integración contable: para cada operación de un módulo, qué cuenta va al Debe y cuál al Haber.
/// Se siembra desde las cuentas fijas de CuentasContablesConst.
/// </summary>
public class MatrizIntegracionContable : BaseEntity
{
    public string ModuloOrigen { get; set; } = string.Empty; // "Compras", "Ventas", "Inventario"
    public string TipoOperacion { get; set; } = string.Empty; // p. ej. "VentaContadoEfectivo"
    public int CuentaDebeId { get; set; }
    public CuentaContable? CuentaDebe { get; set; }
    public int CuentaHaberId { get; set; }
    public CuentaContable? CuentaHaber { get; set; }
    public string? Descripcion { get; set; }
}
