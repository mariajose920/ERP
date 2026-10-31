// Módulo: Inventario — Responsable: Adán
using ERP.Api.Infraestructura;
using ERP.Application.Common;
using ERP.Application.DTOs;
using ERP.Application.Interfaces;
using ERP.Domain.Entities.Inventario;
using ERP.Domain.Entities.Maestros;
using ERP.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

/// <summary>Stock, kardex, ajustes y alertas de inventario.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventarioController : ControllerBase
{
    private readonly IInventarioService _inventarioService;

    public InventarioController(IInventarioService inventarioService)
    {
        _inventarioService = inventarioService;
    }

    /// <summary>Stock actual y CPP de los productos activos.</summary>
    /// <response code="200">Productos.</response>
    [HttpGet("stock")]
    [Authorize(Policy = PermisosConst.InventarioStockLeer)]
    [ProducesResponseType(typeof(List<Producto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStock()
    {
        return Ok(await _inventarioService.GetStockActualAsync());
    }

    /// <summary>Kardex de un producto o kardex general con filtro de fechas, tipo de movimiento y paginación (A7).</summary>
    /// <param name="productoId">Producto (opcional; si se omite devuelve kardex general).</param>
    /// <param name="desde">Fecha inicial (opcional).</param>
    /// <param name="hasta">Fecha final (opcional).</param>
    /// <param name="tipoMovimiento">Tipo de movimiento (opcional).</param>
    /// <param name="pagina">Página (desde 1).</param>
    /// <param name="tamanoPagina">Registros por página (máx. 500).</param>
    /// <response code="200">Página de movimientos, más reciente primero.</response>
    [HttpGet("kardex")]
    [HttpGet("kardex/{productoId:int}")]
    [Authorize(Policy = PermisosConst.InventarioStockLeer)]
    [ProducesResponseType(typeof(PaginaDto<MovimientoKardex>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetKardex(
        int? productoId = null,
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null,
        [FromQuery] TipoMovimientoKardex? tipoMovimiento = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 50)
    {
        return Ok(await _inventarioService.GetKardexAsync(productoId, desde, hasta, tipoMovimiento, pagina, tamanoPagina));
    }

    /// <summary>Registra un ajuste de inventario con kardex y asiento contable (A6).</summary>
    /// <response code="200">Ajuste registrado.</response>
    /// <response code="400">Cantidad 0 o stock resultante negativo.</response>
    /// <response code="404">Producto inexistente.</response>
    [HttpPost("ajustes")]
    [Authorize(Policy = PermisosConst.InventarioAjustesCrear)]
    [ProducesResponseType(typeof(AjusteInventario), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegistrarAjuste([FromBody] RegistrarAjusteInventarioDto dto)
    {
        return Ok(await _inventarioService.RegistrarAjusteAsync(dto, User.ObtenerUsuario()));
    }

    /// <summary>Productos bajo su punto de reorden.</summary>
    /// <response code="200">Productos a reponer.</response>
    [HttpGet("alertas-reorden")]
    [Authorize(Policy = PermisosConst.InventarioStockLeer)]
    [ProducesResponseType(typeof(List<Producto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAlertasReorden()
    {
        return Ok(await _inventarioService.GetAlertasReordenAsync());
    }

    /// <summary>Valorización total del inventario (Σ stock × CPP).</summary>
    /// <response code="200">{ valorTotalInventario }.</response>
    [HttpGet("valorizacion")]
    [Authorize(Policy = PermisosConst.InventarioStockLeer)]
    public async Task<IActionResult> GetValorizacion()
    {
        var total = await _inventarioService.GetValorizacionTotalInventarioAsync();
        return Ok(new { valorTotalInventario = total });
    }

    /// <summary>Últimos ajustes de inventario.</summary>
    /// <param name="top">Cantidad máxima (por defecto 100, máx. 500).</param>
    /// <response code="200">Ajustes con su producto.</response>
    [HttpGet("ajustes")]
    [Authorize(Policy = PermisosConst.InventarioStockLeer)]
    [ProducesResponseType(typeof(List<AjusteInventario>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAjustes([FromQuery] int top = 100)
    {
        return Ok(await _inventarioService.GetAjustesAsync(top));
    }
}
