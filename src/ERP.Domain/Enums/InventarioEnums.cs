// Módulo: Inventario — Responsable: Adán
namespace ERP.Domain.Enums;

/// <summary>Tipo de movimiento registrado en el kardex.</summary>
public enum TipoMovimientoKardex
{
    EntradaCompra = 1,
    SalidaVenta = 2,
    AjusteEntrada = 3,
    AjusteSalida = 4,
    Merma = 5,
    Devolucion = 6
}
