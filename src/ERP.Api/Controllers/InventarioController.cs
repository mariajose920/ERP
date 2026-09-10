using ERP.Application.DTOs;
using ERP.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventarioController : ControllerBase
{
    private readonly IInventarioService _inventarioService;

    public InventarioController(IInventarioService inventarioService)
    {
        _inventarioService = inventarioService;
    }

    [HttpGet("stock")]
    public async Task<IActionResult> GetStock()
    {
        return Ok(await _inventarioService.GetStockActualAsync());
    }

    [HttpGet("kardex/{productoId}")]
    public async Task<IActionResult> GetKardex(int productoId)
    {
        return Ok(await _inventarioService.GetKardexPorProductoAsync(productoId));
    }

    [HttpPost("ajustes")]
    public async Task<IActionResult> RegistrarAjuste([FromBody] RegistrarAjusteInventarioDto dto)
    {
        try
        {
            var usuario = User.Identity?.Name ?? "Cristobal";
            var ajuste = await _inventarioService.RegistrarAjusteAsync(dto, usuario);
            return Ok(ajuste);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("alertas-reorden")]
    public async Task<IActionResult> GetAlertasReorden()
    {
        return Ok(await _inventarioService.GetAlertasReordenAsync());
    }

    [HttpGet("valorizacion")]
    public async Task<IActionResult> GetValorizacion()
    {
        var total = await _inventarioService.GetValorizacionTotalInventarioAsync();
        return Ok(new { valorTotalInventario = total });
    }
}
