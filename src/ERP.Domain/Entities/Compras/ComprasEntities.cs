using ERP.Domain.Common;
using ERP.Domain.Entities.Maestros;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities.Compras;

public class OrdenCompra : BaseEntity
{
    public string NumeroOrden { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; } = DateTime.UtcNow;
    public DateTime? FechaEntregaEsperada { get; set; }
    public int ProveedorId { get; set; }
    public Proveedor? Proveedor { get; set; }
    public EstadoOrdenCompra Estado { get; set; } = EstadoOrdenCompra.Pendiente;
    public decimal Subtotal { get; set; }
    public decimal ImpuestoIva { get; set; }
    public decimal Total { get; set; }
    public string? Observaciones { get; set; }

    public ICollection<DetalleOrdenCompra> Detalles { get; set; } = new List<DetalleOrdenCompra>();
    public ICollection<RecepcionCompra> Recepciones { get; set; } = new List<RecepcionCompra>();
    public ICollection<FacturaCompra> Facturas { get; set; } = new List<FacturaCompra>();
}

public class DetalleOrdenCompra : BaseEntity
{
    public int OrdenCompraId { get; set; }
    public OrdenCompra? OrdenCompra { get; set; }
    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }
    public decimal Cantidad { get; set; }
    public decimal CantidadRecibida { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

public class RecepcionCompra : BaseEntity
{
    public string NumeroRecepcion { get; set; } = string.Empty;
    public DateTime FechaRecepcion { get; set; } = DateTime.UtcNow;
    public int OrdenCompraId { get; set; }
    public OrdenCompra? OrdenCompra { get; set; }
    public string? GuiaDespachoProveedor { get; set; }
    public string? RecibidoPor { get; set; }
    public string? Observaciones { get; set; }

    public ICollection<DetalleRecepcionCompra> Detalles { get; set; } = new List<DetalleRecepcionCompra>();
}

public class DetalleRecepcionCompra : BaseEntity
{
    public int RecepcionCompraId { get; set; }
    public RecepcionCompra? RecepcionCompra { get; set; }
    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }
    public int? DetalleOrdenCompraId { get; set; }
    public DetalleOrdenCompra? DetalleOrdenCompra { get; set; }
    public decimal CantidadRecibida { get; set; }
}

public class FacturaCompra : BaseEntity
{
    public string NumeroFactura { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; } = DateTime.UtcNow;
    public DateTime FechaVencimiento { get; set; } = DateTime.UtcNow.AddDays(30);
    public int ProveedorId { get; set; }
    public Proveedor? Proveedor { get; set; }
    public int? OrdenCompraId { get; set; }
    public OrdenCompra? OrdenCompra { get; set; }
    public int? RecepcionCompraId { get; set; }
    public RecepcionCompra? RecepcionCompra { get; set; }
    public EstadoFacturaCompra Estado { get; set; } = EstadoFacturaCompra.Borrador;
    public decimal Subtotal { get; set; }
    public decimal ImpuestoIva { get; set; }
    public decimal Total { get; set; }
    public int? AsientoContableId { get; set; }

    public ICollection<DetalleFacturaCompra> Detalles { get; set; } = new List<DetalleFacturaCompra>();
}

public class DetalleFacturaCompra : BaseEntity
{
    public int FacturaCompraId { get; set; }
    public FacturaCompra? FacturaCompra { get; set; }
    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}
