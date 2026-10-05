using ERP.Application.DTOs;
using ERP.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VentasController : ControllerBase
{
    private readonly IVentasService _ventasService;

    public VentasController(IVentasService ventasService)
    {
        _ventasService = ventasService;
    }

    [HttpGet]
    public async Task<IActionResult> GetVentas()
    {
        return Ok(await _ventasService.GetVentasAsync());
    }

    [HttpPost]
    public async Task<IActionResult> EmitirVenta([FromBody] CrearVentaDto dto)
    {
        try
        {
            var usuario = User.Identity?.Name ?? "Maria";
            var venta = await _ventasService.EmitirVentaAsync(dto, usuario);
            return Ok(venta);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/anular")]
    public async Task<IActionResult> AnularVenta(int id, [FromBody] string motivo)
    {
        try
        {
            var usuario = User.Identity?.Name ?? "Maria";
            var venta = await _ventasService.AnularVentaAsync(id, motivo, usuario);
            return Ok(venta);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("pagos")]
    public async Task<IActionResult> RegistrarPago([FromBody] RegistrarPagoVentaDto dto)
    {
        try
        {
            var usuario = User.Identity?.Name ?? "Maria";
            var pago = await _ventasService.RegistrarPagoAsync(dto, usuario);
            return Ok(pago);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
