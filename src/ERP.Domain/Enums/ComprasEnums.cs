// Módulo: Compras — Responsable: Adán
namespace ERP.Domain.Enums;

/// <summary>Ciclo de vida de una orden de compra.</summary>
public enum EstadoOrdenCompra
{
    Pendiente = 1,
    RecibidaParcial = 2,
    RecibidaTotal = 3,
    Liquidada = 4,
    Anulada = 5,
    /// <summary>Aprobada por un responsable; habilita la recepción de mercadería (RF-C03).</summary>
    Aprobada = 6
}

/// <summary>Estado de una factura de compra de proveedor.</summary>
public enum EstadoFacturaCompra
{
    Borrador = 1,
    Aprobada = 2,
    Anulada = 3
}
