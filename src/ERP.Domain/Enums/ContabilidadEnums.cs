// Módulo: Contabilidad — Responsable: Cristóbal
namespace ERP.Domain.Enums;

/// <summary>Clasificación de la cuenta dentro del plan de cuentas.</summary>
public enum TipoCuentaContable
{
    Activo = 1,
    Pasivo = 2,
    Patrimonio = 3,
    Ingreso = 4,
    Gasto = 5,
    Costo = 6
}

/// <summary>
/// Naturaleza de la cuenta. Convención: las cuentas deudoras (Activo, Costo, Gasto) aumentan
/// por el Debe y su saldo es Debe − Haber; las acreedoras (Pasivo, Patrimonio, Ingreso)
/// aumentan por el Haber y su saldo es Haber − Debe.
/// </summary>
public enum NaturalezaCuenta
{
    Deudora = 1,
    Acreedora = 2
}

/// <summary>Estado de un período contable.</summary>
public enum EstadoPeriodoContable
{
    Abierto = 1,
    Cerrado = 2
}
