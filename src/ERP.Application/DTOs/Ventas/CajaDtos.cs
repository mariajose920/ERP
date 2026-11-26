// Módulo: Ventas (caja y cobranzas) — Responsable: María
using System.ComponentModel.DataAnnotations;

namespace ERP.Application.DTOs;

/// <summary>Producto tal como lo ve la caja: precio y stock disponible (sin costos).</summary>
public record ProductoCatalogoVentaDto(int Id, string Sku, string Nombre, string? Categoria, decimal PrecioVenta, decimal Stock, decimal PuntoReorden);

/// <summary>Venta con saldo pendiente, con días de mora calculados.</summary>
public record VentaPendienteDto(
    int Id, string NumeroFactura, DateTime FechaEmision, DateTime? FechaVencimiento,
    int ClienteId, string Cliente, string ClienteRut,
    decimal Total, decimal TotalPagado, decimal SaldoPendiente, int DiasMora);

/// <summary>
/// Abono en cascada: un monto que se reparte entre las ventas pendientes del cliente,
/// de la más antigua a la más nueva, hasta agotarlo.
/// </summary>
public record AbonoCascadaDto(
    [Range(1, int.MaxValue)] int ClienteId,
    [Range(typeof(decimal), "0.01", "999999999999", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true, ErrorMessage = "El monto debe ser mayor a 0.")] decimal Monto,
    [Required] string MetodoPago,
    [StringLength(100)] string? ReferenciaComprobante);

/// <summary>Pago aplicado a una venta dentro de un abono en cascada.</summary>
public record PagoAplicadoDto(int VentaId, string NumeroFactura, decimal MontoAplicado, decimal SaldoRestante);

/// <summary>Resultado del abono en cascada. <c>Sobrante</c> &gt; 0 si el monto superó la deuda total.</summary>
public record AbonoCascadaResultadoDto(IReadOnlyList<PagoAplicadoDto> Pagos, decimal TotalAplicado, decimal Sobrante);

/// <summary>Resumen de caja del usuario para el día actual.</summary>
public record ResumenCajaDto(int CantidadVentas, decimal TotalVendido, decimal TotalContado, decimal TotalCredito,
    int CantidadAbonos, decimal TotalAbonos, IReadOnlyList<VentaCajaDto> Ventas,
    decimal VentasEfectivo = 0, decimal AbonosEfectivo = 0, decimal VentasOtrosMedios = 0, decimal AbonosOtrosMedios = 0);

/// <summary>Arqueo de caja: el efectivo esperado se calcula en el servidor.</summary>
public record RegistrarArqueoCajaDto(
    [Range(typeof(decimal), "0", "999999999999", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true, ErrorMessage = "El fondo inicial no puede ser negativo.")] decimal FondoInicial,
    [Range(typeof(decimal), "0", "999999999999", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true, ErrorMessage = "El efectivo contado no puede ser negativo.")] decimal EfectivoContado,
    [StringLength(300)] string? Observaciones);

public record ArqueoCajaDto(int Id, DateTime Fecha, string Usuario, decimal FondoInicial, decimal VentasEfectivo,
    decimal AbonosEfectivo, decimal EfectivoEsperado, decimal EfectivoContado, decimal Diferencia,
    decimal VentasOtrosMedios, string? Observaciones, string? NumeroAsiento);

/// <summary>Venta propia del cajero en el día.</summary>
public record VentaCajaDto(int Id, string NumeroFactura, DateTime FechaEmision, string Cliente, string MetodoPago, string Estado, decimal Total);
