// Módulo: Ventas — Responsable: María
using ERP.Application.DTOs;
using ERP.Domain.Entities.Ventas;

namespace ERP.Application.Interfaces;

/// <summary>Emisión, anulación y cobranza de ventas.</summary>
public interface IVentasService
{
    /// <summary>
    /// Emite una venta: valida stock, cliente activo, descuentos y límite de crédito; descuenta stock,
    /// escribe kardex y genera dos asientos: (a) venta: Debe Caja/Banco/Clientes / Haber Ingresos + IVA DF;
    /// (b) costo: Debe Costo de Ventas / Haber Inventario.
    /// </summary>
    Task<Venta> EmitirVentaAsync(CrearVentaDto dto, string usuario);

    /// <summary>Anula una venta sin abonos: repone stock y genera los asientos de reversa de venta y costo.</summary>
    Task<Venta> AnularVentaAsync(int ventaId, string motivo, string usuario);

    /// <summary>Registra un abono: Debe Caja/Banco / Haber Clientes.</summary>
    Task<PagoVenta> RegistrarPagoAsync(RegistrarPagoVentaDto dto, string usuario);

    /// <summary>Lista las ventas con cliente, detalle y pagos.</summary>
    Task<List<Venta>> GetVentasAsync();

    /// <summary>Ventas con saldo pendiente (opcionalmente de un cliente), con días de mora.</summary>
    Task<List<VentaPendienteDto>> GetVentasPendientesAsync(int? clienteId);

    /// <summary>Reparte un abono entre las ventas pendientes del cliente, de la más antigua a la más nueva.</summary>
    Task<AbonoCascadaResultadoDto> RegistrarAbonoCascadaAsync(AbonoCascadaDto dto, string usuario);

    /// <summary>Ventas y abonos registrados hoy por el usuario indicado (cierre de caja).</summary>
    Task<ResumenCajaDto> GetResumenCajaAsync(string usuario);
}
