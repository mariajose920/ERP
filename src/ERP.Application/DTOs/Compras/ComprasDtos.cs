// Módulo: Compras — Responsable: Adán
using System.ComponentModel.DataAnnotations;

namespace ERP.Application.DTOs;

/// <summary>Datos para emitir una orden de compra.</summary>
public record CrearOrdenCompraDto(
    [Range(1, int.MaxValue)] int ProveedorId,
    DateTime? FechaEntregaEsperada,
    [StringLength(500)] string? Observaciones,
    [Required, MinLength(1, ErrorMessage = "La orden debe tener al menos un ítem.")] List<DetalleOrdenCompraItemDto> Items
);

/// <summary>Línea de una orden de compra.</summary>
public record DetalleOrdenCompraItemDto(
    [Range(1, int.MaxValue)] int ProductoId,
    [Range(typeof(decimal), "0.0001", "999999999999", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true, ErrorMessage = "La cantidad debe ser mayor a 0.")] decimal Cantidad,
    [Range(typeof(decimal), "0", "999999999999", ErrorMessage = "El precio no puede ser negativo.")] decimal PrecioUnitario);

/// <summary>Recepción (total o parcial) de mercadería de una orden de compra. No mueve stock.</summary>
public record RegistrarRecepcionDto(
    [Range(1, int.MaxValue)] int OrdenCompraId,
    [StringLength(50)] string? GuiaDespachoProveedor,
    [StringLength(150)] string? RecibidoPor,
    [StringLength(500)] string? Observaciones,
    [Required, MinLength(1, ErrorMessage = "La recepción debe tener al menos un ítem.")] List<DetalleRecepcionItemDto> Items
);

/// <summary>Cantidad recibida de un producto.</summary>
public record DetalleRecepcionItemDto(
    [Range(1, int.MaxValue)] int ProductoId,
    [Range(typeof(decimal), "0.0001", "999999999999", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true, ErrorMessage = "La cantidad recibida debe ser mayor a 0.")] decimal CantidadRecibida);

/// <summary>
/// Factura de compra que se registra y aprueba en un paso. Regla (A3): el stock entra con la
/// factura (la recepción solo controla cantidades); si la factura viene de una OC, lo facturado
/// no puede superar lo recibido. Genera el asiento Debe Inventario + IVA CF / Haber Proveedores.
/// </summary>
public record AprobarFacturaCompraDto(
    [Range(1, int.MaxValue)] int ProveedorId,
    int? OrdenCompraId,
    int? RecepcionCompraId,
    [Required, StringLength(50)] string NumeroFactura,
    DateTime FechaEmision,
    DateTime FechaVencimiento,
    [Required, MinLength(1, ErrorMessage = "La factura debe tener al menos un ítem.")] List<DetalleFacturaCompraItemDto> Items
);

/// <summary>Línea de una factura de compra.</summary>
public record DetalleFacturaCompraItemDto(
    [Range(1, int.MaxValue)] int ProductoId,
    [Range(typeof(decimal), "0.0001", "999999999999", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true, ErrorMessage = "La cantidad debe ser mayor a 0.")] decimal Cantidad,
    [Range(typeof(decimal), "0", "999999999999", ErrorMessage = "El precio no puede ser negativo.")] decimal PrecioUnitario);

/// <summary>Motivo de anulación de una orden de compra (solo órdenes sin recepciones).</summary>
public record AnularOrdenCompraDto([Required, StringLength(300, MinimumLength = 3)] string Motivo);

/// <summary>Motivo de anulación de una factura de compra (obligatorio).</summary>
public record AnularFacturaCompraDto([Required, StringLength(300, MinimumLength = 3)] string Motivo);

/// <summary>Pago a proveedor de una factura de compra aprobada (espejo de RegistrarPagoVentaDto).</summary>
public record RegistrarPagoCompraDto(
    [Range(1, int.MaxValue)] int CompraProveedorId,
    [Range(typeof(decimal), "0.01", "999999999999", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true, ErrorMessage = "El monto debe ser mayor a 0.")] decimal Monto,
    [Required] string MetodoPago,
    [StringLength(100)] string? ReferenciaComprobante);
