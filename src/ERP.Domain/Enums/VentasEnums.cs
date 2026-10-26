// Módulo: Ventas — Responsable: María
namespace ERP.Domain.Enums;

/// <summary>Estado de una venta (factura de venta).</summary>
public enum EstadoVenta
{
    Emitida = 1,
    Pagada = 2,
    ParcialmentePagada = 3,
    Anulada = 4
}
