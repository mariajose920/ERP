using ERP.Domain.Common;
using ERP.Domain.Entities.Maestros;

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
