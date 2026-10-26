// Módulo: Contabilidad — Responsable: Cristóbal
using System.ComponentModel.DataAnnotations;
using ERP.Domain.Enums;

namespace ERP.Application.DTOs;

/// <summary>Asiento manual. Debe tener al menos 2 líneas y cuadrar (Σ Debe = Σ Haber).</summary>
public record CrearAsientoManualDto(
    [Required, StringLength(300)] string Glosa,
    DateTime Fecha,
    [Required, MinLength(2, ErrorMessage = "Un asiento requiere al menos 2 líneas.")] List<DetalleAsientoManualItemDto> Lineas
);

/// <summary>Línea de asiento manual: exactamente uno de Debe/Haber debe ser mayor a 0.</summary>
public record DetalleAsientoManualItemDto(
    [Range(1, int.MaxValue)] int CuentaContableId,
    [Range(typeof(decimal), "0", "999999999999")] decimal Debe,
    [Range(typeof(decimal), "0", "999999999999")] decimal Haber,
    [StringLength(300)] string? GlosaLinea);

/// <summary>Línea de un asiento automático, identificada por el código de cuenta.</summary>
public record LineaAsientoAutomaticoDto(string CodigoCuenta, decimal Debe, decimal Haber, string? GlosaLinea);

/// <summary>Asiento generado por otro módulo (Compras, Ventas, Inventario).</summary>
public record AsientoAutomaticoDto(
    string Glosa,
    string ModuloOrigen,
    string? DocumentoOrigenTipo,
    int? DocumentoOrigenId,
    IReadOnlyList<LineaAsientoAutomaticoDto> Lineas);

/// <summary>Fila del balance de comprobación.</summary>
public record BalanceComprobacionFilaDto(
    string CodigoCuenta,
    string NombreCuenta,
    decimal TotalDebe,
    decimal TotalHaber,
    decimal SaldoDeudor,
    decimal SaldoAcreedor
);

/// <summary>Estado de resultados del período.</summary>
public record EstadoResultadosDto(
    decimal IngresosPorVentas,
    decimal CostoDeVentas,
    decimal MargenBruto,
    decimal GastosOperacionales,
    decimal UtilidadNeta
);

/// <summary>Datos para crear una cuenta contable.</summary>
public record CrearCuentaContableDto(
    [Required, StringLength(20)] string Codigo,
    [Required, StringLength(200)] string Nombre,
    TipoCuentaContable Tipo,
    NaturalezaCuenta Naturaleza,
    [Range(1, 9)] int Nivel,
    int? CuentaPadreId,
    bool PermiteMovimiento);

/// <summary>Línea del balance general (cuenta con su saldo según naturaleza).</summary>
public record LineaBalanceGeneralDto(string Codigo, string Nombre, TipoCuentaContable Tipo, decimal Saldo);

/// <summary>
/// Balance general a la fecha: Activo = Pasivo + Patrimonio + Resultado del ejercicio
/// (ingresos − costos − gastos aún no cerrados).
/// </summary>
public record BalanceGeneralDto(
    decimal TotalActivos, decimal TotalPasivos, decimal TotalPatrimonio, decimal ResultadoEjercicio,
    bool Cuadra, IReadOnlyList<LineaBalanceGeneralDto> Lineas);

/// <summary>Cambios permitidos sobre una cuenta existente (el código y la naturaleza no se modifican).</summary>
public record ActualizarCuentaContableDto(
    [Required, StringLength(200)] string Nombre,
    bool PermiteMovimiento,
    bool Activo);

/// <summary>Período contable mensual. Id = 0 si el período aún no tiene fila (se considera Abierto).</summary>
public record PeriodoContableDto(
    int Id, int Anio, int Mes, DateTime FechaInicio, DateTime FechaFin, string Estado,
    DateTime? FechaCierre, string? CerradoPor, int CantidadAsientos);

/// <summary>Fila de la matriz de integración contable con los datos de sus cuentas.</summary>
public record MatrizIntegracionDto(
    int Id, string ModuloOrigen, string TipoOperacion, string? Descripcion,
    int CuentaDebeId, string CuentaDebeCodigo, string CuentaDebeNombre,
    int CuentaHaberId, string CuentaHaberCodigo, string CuentaHaberNombre, bool Activo);

/// <summary>Alta de una fila de la matriz.</summary>
public record CrearMatrizIntegracionDto(
    [Required, StringLength(50)] string ModuloOrigen,
    [Required, StringLength(100)] string TipoOperacion,
    [Range(1, int.MaxValue)] int CuentaDebeId,
    [Range(1, int.MaxValue)] int CuentaHaberId,
    [StringLength(300)] string? Descripcion);

/// <summary>Cambios sobre una fila de la matriz (módulo y operación no se modifican).</summary>
public record ActualizarMatrizIntegracionDto(
    [Range(1, int.MaxValue)] int CuentaDebeId,
    [Range(1, int.MaxValue)] int CuentaHaberId,
    [StringLength(300)] string? Descripcion,
    bool Activo);
