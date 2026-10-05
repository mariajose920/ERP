using ERP.Application.DTOs;
using ERP.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ComprasController : ControllerBase
{
    private readonly IComprasService _comprasService;

    public ComprasController(IComprasService comprasService)
    {
        _comprasService = comprasService;
    }

    [HttpGet("ordenes")]
    public async Task<IActionResult> GetOrdenes()
    {
        return Ok(await _comprasService.GetOrdenesAsync());
    }

    [HttpPost("ordenes")]
    public async Task<IActionResult> CrearOrden([FromBody] CrearOrdenCompraDto dto)
    {
        var usuario = User.Identity?.Name ?? "Adan";
        var orden = await _comprasService.CrearOrdenCompraAsync(dto, usuario);
        return Ok(orden);
    }

    [HttpPost("recepciones")]
    public async Task<IActionResult> RegistrarRecepcion([FromBody] RegistrarRecepcionDto dto)
    {
        var usuario = User.Identity?.Name ?? "Adan";
        var recepcion = await _comprasService.RegistrarRecepcionAsync(dto, usuario);
        return Ok(recepcion);
    }

    [HttpGet("facturas")]
    public async Task<IActionResult> GetFacturas()
    {
        return Ok(await _comprasService.GetFacturasAsync());
    }

    [HttpPost("facturas")]
    public async Task<IActionResult> RegistrarFactura([FromBody] AprobarFacturaCompraDto dto)
    {
        var usuario = User.Identity?.Name ?? "Adan";
        var factura = await _comprasService.RegistrarYAprobarFacturaCompraAsync(dto, usuario);
        return Ok(factura);
    }
}
