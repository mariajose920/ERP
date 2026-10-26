// Módulo: Contabilidad — Responsable: Cristóbal
using ERP.Application.DTOs;
using ERP.Domain.Entities.Contabilidad;

namespace ERP.Application.Interfaces;

/// <summary>
/// Contabilidad general. Es el ÚNICO punto que crea asientos y modifica SaldoActual de las cuentas (K3),
/// respetando la naturaleza deudora/acreedora de cada cuenta.
/// </summary>
public interface IContabilidadService
{
    /// <summary>Crea un asiento manual validado (≥2 líneas, cuadrado, cuentas existentes con PermiteMovimiento).</summary>
    Task<AsientoContable> CrearAsientoAsync(CrearAsientoManualDto dto, string usuario);

    /// <summary>
    /// Crea un asiento automático desde otro módulo. Debe llamarse dentro de la transacción del
    /// documento de origen. Las líneas con Debe y Haber en 0 se omiten.
    /// </summary>
    Task<AsientoContable> RegistrarAsientoAutomaticoAsync(AsientoAutomaticoDto dto, string usuario);

    /// <summary>Genera el asiento inverso (Debe↔Haber) de un asiento existente.</summary>
    Task<AsientoContable> RevertirAsientoAsync(int asientoId, string glosa, string usuario);

    /// <summary>Libro diario entre fechas.</summary>
    Task<List<AsientoContable>> GetLibroDiarioAsync(DateTime? desde, DateTime? hasta);

    /// <summary>Movimientos de una cuenta entre fechas.</summary>
    Task<List<DetalleAsientoContable>> GetLibroMayorPorCuentaAsync(int cuentaId, DateTime? desde, DateTime? hasta);

    /// <summary>Balance de comprobación (sumas y saldos), agrupado en SQL.</summary>
    Task<List<BalanceComprobacionFilaDto>> GetBalanceComprobacionAsync();

    /// <summary>Estado de resultados del período.</summary>
    Task<EstadoResultadosDto> GetEstadoResultadosAsync(DateTime? desde, DateTime? hasta);

    /// <summary>Balance general a la fecha a partir de los saldos de las cuentas.</summary>
    Task<BalanceGeneralDto> GetBalanceGeneralAsync();

    /// <summary>Lanza InvalidOperationException si el mes de <paramref name="fecha"/> pertenece a un período cerrado.</summary>
    Task ValidarPeriodoAbiertoAsync(DateTime fecha);

    /// <summary>Los 12 períodos del año (los que no tienen fila se informan como Abierto con Id = 0).</summary>
    Task<List<PeriodoContableDto>> GetPeriodosAsync(int anio);

    /// <summary>Cierra el período (lo crea si no existe). Los asientos fechados en él quedan bloqueados.</summary>
    Task<PeriodoContable> CerrarPeriodoAsync(int anio, int mes, string usuario);

    /// <summary>Reabre un período cerrado.</summary>
    Task<PeriodoContable> ReabrirPeriodoAsync(int anio, int mes, string usuario);

    /// <summary>
    /// Códigos de cuenta Debe/Haber de una operación según la matriz de integración contable;
    /// si la fila no existe o está inactiva se usan los valores por defecto (cuentas fijas de CuentasContablesConst).
    /// </summary>
    Task<(string CodigoDebe, string CodigoHaber)> ResolverCuentasAsync(string modulo, string operacion, string codigoDebePorDefecto, string codigoHaberPorDefecto);
}
