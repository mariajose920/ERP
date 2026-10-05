using ERP.Application.DTOs;
using ERP.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContabilidadController : ControllerBase
{
    private readonly IContabilidadService _contabilidadService;

    public ContabilidadController(IContabilidadService contabilidadService)
    {
        _contabilidadService = contabilidadService;
    }

    [HttpPost("asientos")]
    public async Task<IActionResult> CrearAsiento([FromBody] CrearAsientoManualDto dto)
    {
        try
        {
            var usuario = User.Identity?.Name ?? "Cristobal";
            var asiento = await _contabilidadService.CrearAsientoAsync(dto, usuario);
            return Ok(asiento);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("libro-diario")]
    public async Task<IActionResult> GetLibroDiario([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        return Ok(await _contabilidadService.GetLibroDiarioAsync(desde, hasta));
    }

    [HttpGet("libro-mayor/{cuentaId}")]
    public async Task<IActionResult> GetLibroMayor(int cuentaId, [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        return Ok(await _contabilidadService.GetLibroMayorPorCuentaAsync(cuentaId, desde, hasta));
    }

    [HttpGet("balance-comprobacion")]
    public async Task<IActionResult> GetBalanceComprobacion()
    {
        return Ok(await _contabilidadService.GetBalanceComprobacionAsync());
    }

    [HttpGet("estado-resultados")]
    public async Task<IActionResult> GetEstadoResultados([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        return Ok(await _contabilidadService.GetEstadoResultadosAsync(desde, hasta));
    }
}
