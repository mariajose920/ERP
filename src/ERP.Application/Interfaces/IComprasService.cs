// Módulo: Compras — Responsable: Adán
using ERP.Application.DTOs;
using ERP.Domain.Entities.Compras;

namespace ERP.Application.Interfaces;

/// <summary>Ciclo de compras: orden → recepción → factura.</summary>
public interface IComprasService
{
    /// <summary>Crea una orden de compra (estado Pendiente) con IVA calculado.</summary>
    Task<OrdenCompra> CrearOrdenCompraAsync(CrearOrdenCompraDto dto, string usuario);

    /// <summary>Registra una recepción parcial o total. No mueve stock; actualiza CantidadRecibida y el estado de la OC.</summary>
    Task<RecepcionCompra> RegistrarRecepcionAsync(RegistrarRecepcionDto dto, string usuario);

    /// <summary>
    /// Registra y aprueba una factura de compra: ingresa stock, recalcula el CPP de cada producto,
    /// escribe el kardex y genera el asiento Debe Inventario (neto) + Debe IVA Crédito Fiscal (IVA)
    /// / Haber Proveedores (total).
    /// </summary>
    Task<FacturaCompra> RegistrarYAprobarFacturaCompraAsync(AprobarFacturaCompraDto dto, string usuario);

    /// <summary>Lista las órdenes de compra con proveedor y detalle.</summary>
    Task<List<OrdenCompra>> GetOrdenesAsync();

    /// <summary>Lista las facturas de compra con proveedor y detalle.</summary>
    Task<List<FacturaCompra>> GetFacturasAsync();

    /// <summary>Anula una orden de compra sin recepciones ni facturas.</summary>
    Task<OrdenCompra> AnularOrdenAsync(int ordenId, string motivo, string usuario);

    /// <summary>Aprueba una orden Pendiente (habilita su recepción).</summary>
    Task<OrdenCompra> AprobarOrdenAsync(int ordenId, string usuario);

    /// <summary>Lista las recepciones con su orden y detalle.</summary>
    Task<List<RecepcionCompra>> GetRecepcionesAsync();

    /// <summary>
    /// Anula una factura de compra aprobada sin pagos: exige motivo, saca del stock lo ingresado, revierte el asiento
    /// (enlazando AsientoRevertidoId) y deja la OC lista para refacturar.
    /// </summary>
    Task<FacturaCompra> AnularFacturaAsync(int facturaId, string motivo, string usuario);

    /// <summary>
    /// Registra un pago a proveedor: actualiza TotalPagado/SaldoPendiente de la factura y genera el asiento
    /// Debe Proveedores / Haber Caja o Banco (espejo del abono de clientes).
    /// </summary>
    Task<PagoProveedor> RegistrarPagoAsync(RegistrarPagoCompraDto dto, string usuario);

    /// <summary>Lista los pagos a proveedores (opcionalmente de una factura).</summary>
    Task<List<PagoProveedor>> GetPagosAsync(int? facturaId);
}
