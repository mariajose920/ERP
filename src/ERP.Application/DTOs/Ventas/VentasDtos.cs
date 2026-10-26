// Módulo: Ventas — Responsable: María
using System.ComponentModel.DataAnnotations;

namespace ERP.Application.DTOs;

/// <summary>Datos para emitir una venta desde el POS.</summary>
/// <param name="ClienteId">Cliente activo.</param>
/// <param name="MetodoPago">Uno de <c>MetodosPago</c>: Efectivo, Transferencia, Tarjeta o Credito.</param>
/// <param name="Observaciones">Texto libre opcional.</param>
/// <param name="Items">Líneas de la venta (al menos una).</param>
public record CrearVentaDto(
    int? ClienteId,
    [Required] string MetodoPago,
    [StringLength(500)] string? Observaciones,
    [Required, MinLength(1, ErrorMessage = "La venta debe tener al menos un ítem.")] List<DetalleVentaItemDto> Items
);

/// <summary>Línea de venta. El descuento es un monto (no porcentaje) y no puede superar Cantidad × Precio.</summary>
public record DetalleVentaItemDto(
    [Range(1, int.MaxValue)] int ProductoId,
    [Range(typeof(decimal), "0.0001", "999999999999", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true, ErrorMessage = "La cantidad debe ser mayor a 0.")] decimal Cantidad,
    [Range(typeof(decimal), "0", "999999999999", ErrorMessage = "El precio no puede ser negativo.")] decimal PrecioUnitario,
    [Range(typeof(decimal), "0", "999999999999", ErrorMessage = "El descuento no puede ser negativo.")] decimal Descuento);

/// <summary>Abono a una venta a crédito.</summary>
public record RegistrarPagoVentaDto(
    [Range(1, int.MaxValue)] int VentaId,
    [Range(typeof(decimal), "0.01", "999999999999", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true, ErrorMessage = "El monto debe ser mayor a 0.")] decimal Monto,
    [Required] string MetodoPago,
    [StringLength(100)] string? ReferenciaComprobante
);

/// <summary>Motivo de anulación de una venta (M6).</summary>
public record AnularVentaDto([Required, StringLength(300, MinimumLength = 3)] string Motivo);
