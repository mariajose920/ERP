// Módulo: Compras — Responsable: Adán
using ERP.Api.Infraestructura;
using ERP.Application.Common;
using ERP.Application.DTOs;
using ERP.Application.Interfaces;
using ERP.Domain.Entities.Compras;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

/// <summary>Órdenes de compra, recepciones y facturas de proveedor.</summary>
/// <remarks>Los DTO se validan con DataAnnotations (A1): un cuerpo inválido devuelve 400 con el detalle.</remarks>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ComprasController : ControllerBase
{
    private readonly IComprasService _comprasService;

    public ComprasController(IComprasService comprasService)
    {
        _comprasService = comprasService;
    }

    /// <summary>Lista las órdenes de compra.</summary>
    /// <response code="200">Órdenes con proveedor y detalle.</response>
    [HttpGet("ordenes")]
    [Authorize(Policy = PermisosConst.ComprasOrdenesLeer)]
    [ProducesResponseType(typeof(List<OrdenCompra>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrdenes()
    {
        return Ok(await _comprasService.GetOrdenesAsync());
    }

    /// <summary>Crea una orden de compra.</summary>
    /// <response code="200">Orden creada (estado Pendiente).</response>
    /// <response code="400">Datos inválidos, proveedor o productos inexistentes.</response>
    [HttpPost("ordenes")]
    [Authorize(Policy = PermisosConst.ComprasOrdenesEscribir)]
    [ProducesResponseType(typeof(OrdenCompra), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearOrden([FromBody] CrearOrdenCompraDto dto)
    {
        return Ok(await _comprasService.CrearOrdenCompraAsync(dto, User.ObtenerUsuario()));
    }

    /// <summary>Registra la recepción (parcial o total) de una orden. No mueve stock.</summary>
    /// <response code="200">Recepción registrada.</response>
    /// <response code="400">Cantidades mayores a lo pendiente o producto ajeno a la orden.</response>
    /// <response code="404">Orden inexistente.</response>
    [HttpPost("recepciones")]
    [Authorize(Policy = PermisosConst.ComprasRecepcionesCrear)]
    [ProducesResponseType(typeof(RecepcionCompra), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegistrarRecepcion([FromBody] RegistrarRecepcionDto dto)
    {
        return Ok(await _comprasService.RegistrarRecepcionAsync(dto, User.ObtenerUsuario()));
    }

    /// <summary>Lista las facturas de compra.</summary>
    /// <response code="200">Facturas con proveedor y detalle.</response>
    [HttpGet("facturas")]
    [Authorize(Policy = PermisosConst.ComprasOrdenesLeer)]
    [ProducesResponseType(typeof(List<FacturaCompra>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFacturas()
    {
        return Ok(await _comprasService.GetFacturasAsync());
    }

    /// <summary>
    /// Registra y aprueba una factura de compra: ingresa stock, recalcula CPP y genera el asiento
    /// Debe Inventario + IVA Crédito Fiscal / Haber Proveedores.
    /// </summary>
    /// <response code="200">Factura aprobada.</response>
    /// <response code="400">Factura duplicada, OC de otro proveedor o cantidades facturadas mayores a lo recibido.</response>
    [HttpPost("facturas")]
    [Authorize(Policy = PermisosConst.ComprasFacturasCrear)]
    [ProducesResponseType(typeof(FacturaCompra), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrarFactura([FromBody] AprobarFacturaCompraDto dto)
    {
        return Ok(await _comprasService.RegistrarYAprobarFacturaCompraAsync(dto, User.ObtenerUsuario()));
    }

    /// <summary>Anula una orden de compra sin recepciones ni facturas.</summary>
    /// <response code="200">Orden anulada.</response>
    /// <response code="400">La orden ya tiene recepciones/facturas o ya estaba anulada.</response>
    /// <response code="404">Orden inexistente.</response>
    [HttpPost("ordenes/{id:int}/anular")]
    [Authorize(Policy = PermisosConst.ComprasOrdenesEscribir)]
    [ProducesResponseType(typeof(OrdenCompra), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AnularOrden(int id, [FromBody] AnularOrdenCompraDto dto)
    {
        return Ok(await _comprasService.AnularOrdenAsync(id, dto.Motivo, User.ObtenerUsuario()));
    }

    /// <summary>Aprueba una orden de compra Pendiente; solo una orden aprobada admite recepciones.</summary>
    /// <response code="200">Orden aprobada.</response>
    /// <response code="400">La orden no está Pendiente.</response>
    /// <response code="404">Orden inexistente.</response>
    [HttpPost("ordenes/{id:int}/aprobar")]
    [Authorize(Policy = PermisosConst.ComprasOrdenesEscribir)]
    [ProducesResponseType(typeof(OrdenCompra), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AprobarOrden(int id)
    {
        return Ok(await _comprasService.AprobarOrdenAsync(id, User.ObtenerUsuario()));
    }

    /// <summary>Lista las recepciones de mercadería con su orden y detalle.</summary>
    /// <response code="200">Recepciones, más reciente primero.</response>
    [HttpGet("recepciones")]
    [Authorize(Policy = PermisosConst.ComprasOrdenesLeer)]
    [ProducesResponseType(typeof(List<RecepcionCompra>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecepciones()
    {
        return Ok(await _comprasService.GetRecepcionesAsync());
    }

    /// <summary>Anula una factura de compra aprobada sin pagos (motivo obligatorio): saca el stock y revierte el asiento.</summary>
    /// <response code="200">Factura anulada.</response>
    /// <response code="400">Sin motivo, ya anulada, con pagos o con stock ya consumido.</response>
    /// <response code="404">Factura inexistente.</response>
    [HttpPost("facturas/{id:int}/anular")]
    [Authorize(Policy = PermisosConst.ComprasFacturasCrear)]
    [ProducesResponseType(typeof(FacturaCompra), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AnularFactura(int id, [FromBody] AnularFacturaCompraDto dto)
    {
        return Ok(await _comprasService.AnularFacturaAsync(id, dto.Motivo, User.ObtenerUsuario()));
    }

    /// <summary>Lista los pagos a proveedores, opcionalmente de una factura.</summary>
    /// <response code="200">Pagos, más reciente primero.</response>
    [HttpGet("pagos")]
    [Authorize(Policy = PermisosConst.ComprasOrdenesLeer)]
    [ProducesResponseType(typeof(List<PagoProveedor>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPagos([FromQuery] int? facturaId)
    {
        return Ok(await _comprasService.GetPagosAsync(facturaId));
    }

    /// <summary>
    /// Registra un pago a proveedor: actualiza TotalPagado/SaldoPendiente de la factura y genera el asiento
    /// Debe Proveedores / Haber Caja o Banco.
    /// </summary>
    /// <response code="200">Pago registrado con su asiento.</response>
    /// <response code="400">Monto mayor al saldo, factura no aprobada, método inválido o período contable cerrado.</response>
    /// <response code="404">Factura inexistente.</response>
    [HttpPost("pagos")]
    [Authorize(Policy = PermisosConst.ComprasPagosCrear)]
    [ProducesResponseType(typeof(PagoProveedor), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegistrarPago([FromBody] RegistrarPagoCompraDto dto)
    {
        return Ok(await _comprasService.RegistrarPagoAsync(dto, User.ObtenerUsuario()));
    }
}
