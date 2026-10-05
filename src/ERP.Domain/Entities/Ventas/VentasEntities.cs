using ERP.Domain.Common;
using ERP.Domain.Entities.Maestros;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities.Ventas;

public class Venta : BaseEntity
{
    public string NumeroFactura { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; } = DateTime.UtcNow;
    public DateTime? FechaVencimiento { get; set; }
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public EstadoVenta Estado { get; set; } = EstadoVenta.Emitida;
    public decimal Subtotal { get; set; }
    public decimal ImpuestoIva { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }
    public decimal TotalPagado { get; set; }
    public decimal SaldoPendiente { get; set; }
    public decimal CostoVentaTotal { get; set; } // Suma de costo CPP de productos vendidos
    public string MetodoPago { get; set; } = "Efectivo"; // Efectivo, Transferencia, Tarjeta, Crédito
    public int? AsientoVentaId { get; set; }
    public int? AsientoCostoVentaId { get; set; }
    public string? Observaciones { get; set; }

    public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
    public ICollection<PagoVenta> Pagos { get; set; } = new List<PagoVenta>();
}

public class DetalleVenta : BaseEntity
{
    public int VentaId { get; set; }
    public Venta? Venta { get; set; }
    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal CostoUnitarioSnapshot { get; set; } // CPP al momento de vender
    public decimal Descuento { get; set; }
    public decimal Subtotal { get; set; }
}

public class PagoVenta : BaseEntity
{
    public int VentaId { get; set; }
    public Venta? Venta { get; set; }
    public DateTime FechaPago { get; set; } = DateTime.UtcNow;
    public decimal Monto { get; set; }
    public string MetodoPago { get; set; } = "Transferencia";
    public string? ReferenciaComprobante { get; set; }
    public int? AsientoPagoId { get; set; }
}
