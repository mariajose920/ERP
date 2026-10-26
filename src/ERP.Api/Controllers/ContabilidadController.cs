// Módulo: Contabilidad — Responsable: Cristóbal
using ERP.Api.Infraestructura;
using ERP.Application.Common;
using ERP.Application.DTOs;
using ERP.Application.Interfaces;
using ERP.Domain.Entities.Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

/// <summary>
/// Asientos, libros y estados financieros. Convención de saldos: cuentas deudoras (Activo, Costo, Gasto)
/// = Debe − Haber; cuentas acreedoras (Pasivo, Patrimonio, Ingreso) = Haber − Debe.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContabilidadController : ControllerBase
{
    private readonly IContabilidadService _contabilidadService;

    public ContabilidadController(IContabilidadService contabilidadService)
    {
        _contabilidadService = contabilidadService;
    }

    /// <summary>Crea un asiento manual (≥2 líneas, cuadrado, cada línea solo Debe o solo Haber).</summary>
    /// <response code="200">Asiento creado.</response>
    /// <response code="400">Asiento descuadrado, línea inválida o cuenta sin movimiento.</response>
    [HttpPost("asientos")]
    [Authorize(Policy = PermisosConst.ContabilidadAsientosCrear)]
    [ProducesResponseType(typeof(AsientoContable), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearAsiento([FromBody] CrearAsientoManualDto dto)
    {
        return Ok(await _contabilidadService.CrearAsientoAsync(dto, User.ObtenerUsuario()));
    }

    /// <summary>Libro diario: asientos con sus líneas entre fechas (opcionales).</summary>
    /// <response code="200">Lista de asientos, más reciente primero.</response>
    [HttpGet("libro-diario")]
    [Authorize(Policy = PermisosConst.ContabilidadAsientosLeer)]
    [ProducesResponseType(typeof(List<AsientoContable>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLibroDiario([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        return Ok(await _contabilidadService.GetLibroDiarioAsync(desde, hasta));
    }

    /// <summary>Libro mayor de una cuenta entre fechas (opcionales).</summary>
    /// <response code="200">Movimientos de la cuenta en orden cronológico.</response>
    [HttpGet("libro-mayor/{cuentaId}")]
    [Authorize(Policy = PermisosConst.ContabilidadAsientosLeer)]
    [ProducesResponseType(typeof(List<DetalleAsientoContable>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLibroMayor(int cuentaId, [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        return Ok(await _contabilidadService.GetLibroMayorPorCuentaAsync(cuentaId, desde, hasta));
    }

    /// <summary>Balance de comprobación (sumas y saldos por cuenta).</summary>
    /// <response code="200">Una fila por cuenta del plan.</response>
    [HttpGet("balance-comprobacion")]
    [Authorize(Policy = PermisosConst.ContabilidadReportesLeer)]
    [ProducesResponseType(typeof(List<BalanceComprobacionFilaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBalanceComprobacion()
    {
        return Ok(await _contabilidadService.GetBalanceComprobacionAsync());
    }

    /// <summary>Estado de resultados del período.</summary>
    /// <response code="200">Ingresos, costo, margen, gastos y utilidad.</response>
    [HttpGet("estado-resultados")]
    [Authorize(Policy = PermisosConst.ContabilidadReportesLeer)]
    [ProducesResponseType(typeof(EstadoResultadosDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEstadoResultados([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        return Ok(await _contabilidadService.GetEstadoResultadosAsync(desde, hasta));
    }

    /// <summary>Balance general a la fecha (Activo = Pasivo + Patrimonio + Resultado del ejercicio).</summary>
    /// <response code="200">Totales por grupo y detalle de cuentas con saldo.</response>
    [HttpGet("balance-general")]
    [Authorize(Policy = PermisosConst.ContabilidadReportesLeer)]
    [ProducesResponseType(typeof(BalanceGeneralDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBalanceGeneral()
    {
        return Ok(await _contabilidadService.GetBalanceGeneralAsync());
    }

    /// <summary>Períodos contables mensuales de un año (los sin fila se informan como Abierto).</summary>
    /// <param name="anio">Año; por defecto el actual.</param>
    /// <response code="200">Los 12 meses con su estado y cantidad de asientos.</response>
    [HttpGet("periodos")]
    [Authorize(Policy = PermisosConst.ContabilidadAsientosLeer)]
    [ProducesResponseType(typeof(List<PeriodoContableDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPeriodos([FromQuery] int? anio)
    {
        return Ok(await _contabilidadService.GetPeriodosAsync(anio ?? DateTime.UtcNow.Year));
    }

    /// <summary>Cierra un período: no se podrán crear asientos (manuales ni automáticos) fechados en ese mes.</summary>
    /// <response code="200">Período cerrado.</response>
    /// <response code="400">Período ya cerrado o año/mes inválido.</response>
    [HttpPost("periodos/{anio:int}/{mes:int}/cerrar")]
    [Authorize(Policy = PermisosConst.ContabilidadPeriodosAdministrar)]
    [ProducesResponseType(typeof(PeriodoContable), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CerrarPeriodo(int anio, int mes)
    {
        return Ok(await _contabilidadService.CerrarPeriodoAsync(anio, mes, User.ObtenerUsuario()));
    }

    /// <summary>Reabre un período cerrado.</summary>
    /// <response code="200">Período reabierto.</response>
    /// <response code="400">El período no está cerrado.</response>
    [HttpPost("periodos/{anio:int}/{mes:int}/reabrir")]
    [Authorize(Policy = PermisosConst.ContabilidadPeriodosAdministrar)]
    [ProducesResponseType(typeof(PeriodoContable), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReabrirPeriodo(int anio, int mes)
    {
        return Ok(await _contabilidadService.ReabrirPeriodoAsync(anio, mes, User.ObtenerUsuario()));
    }
}
