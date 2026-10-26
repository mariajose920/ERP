// Módulo: Contabilidad (maestros) — Responsable: Cristóbal
using ERP.Domain.Common;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities.Maestros;

/// <summary>
/// Cuenta del plan de cuentas. <see cref="SaldoActual"/> se expresa según la naturaleza de la cuenta
/// (ver <see cref="NaturalezaCuenta"/>) y solo debe modificarse mediante
/// ContabilidadService.AplicarMovimiento.
/// </summary>
public class CuentaContable : BaseEntity
{
    public string Codigo { get; set; } = string.Empty; // p. ej. "1.1.01.01"
    public string Nombre { get; set; } = string.Empty;
    public TipoCuentaContable Tipo { get; set; }
    public NaturalezaCuenta Naturaleza { get; set; }
    public int Nivel { get; set; } = 1;
    public int? CuentaPadreId { get; set; }
    public CuentaContable? CuentaPadre { get; set; }
    public bool PermiteMovimiento { get; set; } = true;
    public decimal SaldoActual { get; set; }
}
