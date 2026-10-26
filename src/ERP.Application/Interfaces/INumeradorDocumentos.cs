// Módulo: Transversal (T6) — Responsable: Cristóbal
namespace ERP.Application.Interfaces;

/// <summary>Tipos de documento numerados por el sistema.</summary>
public enum TipoDocumentoNumerado
{
    /// <summary>Orden de compra: OC-AAAA-NNNNN.</summary>
    OrdenCompra,
    /// <summary>Recepción de compra: RC-AAAA-NNNNN.</summary>
    RecepcionCompra,
    /// <summary>Factura de venta: FV-AAAA-NNNNN.</summary>
    FacturaVenta,
    /// <summary>Asiento contable: AS-AAAA-NNNNN.</summary>
    AsientoContable,
    /// <summary>Ajuste de inventario: AJ-AAAA-NNNNN.</summary>
    AjusteInventario
}

/// <summary>
/// Entrega números de documento correlativos sin duplicados (T6). Reemplaza el patrón
/// <c>CountAsync() + 1</c>, que duplicaba números con concurrencia o tras anulaciones.
/// </summary>
public interface INumeradorDocumentos
{
    /// <summary>
    /// Devuelve el siguiente número del tipo indicado para el año en curso. Debe llamarse dentro de
    /// una transacción: toma un bloqueo exclusivo (sp_getapplock) que se libera al confirmar o revertir.
    /// </summary>
    Task<string> SiguienteAsync(TipoDocumentoNumerado tipo);
}
