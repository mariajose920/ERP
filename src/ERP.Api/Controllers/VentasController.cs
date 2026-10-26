// Módulo: Ventas — Responsable: María
using ERP.Api.Infraestructura;
using ERP.Application.Common;
using ERP.Application.DTOs;
using ERP.Application.Interfaces;
using ERP.Domain.Entities.Ventas;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

/// <summary>Emisión, anulación y cobranza de ventas.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VentasController : ControllerBase
{
    private readonly IVentasService _ventasService;

    public VentasController(IVentasService ventasService)
    {
        _ventasService = ventasService;
    }

    /// <summary>Lista las ventas con cliente, detalle y pagos.</summary>
    /// <response code="200">Ventas, más reciente primero.</response>
    [HttpGet]
    [Authorize(Policy = PermisosConst.VentasDocumentosLeer)]
    [ProducesResponseType(typeof(List<Venta>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVentas()
    {
        return Ok(await _ventasService.GetVentasAsync());
    }

    /// <summary>
    /// Emite una venta. Métodos de pago: Efectivo, Transferencia, Tarjeta (pagadas al emitir) o
    /// Credito (queda saldo pendiente, vence a 30 días y respeta el límite de crédito).
    /// </summary>
    /// <response code="200">Venta emitida con sus asientos.</response>
    /// <response code="400">Stock insuficiente, descuento inválido, cliente inactivo o límite de crédito excedido.</response>
    [HttpPost]
    [Authorize(Policy = PermisosConst.VentasDocumentosCrear)]
    [ProducesResponseType(typeof(Venta), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EmitirVenta([FromBody] CrearVentaDto dto)
    {
        return Ok(await _ventasService.EmitirVentaAsync(dto, User.ObtenerUsuario()));
    }

    /// <summary>Anula una venta sin abonos: repone stock y genera asientos de reversa (M2).</summary>
    /// <param name="id">Id de la venta.</param>
    /// <param name="dto">Motivo de la anulación (antes era un string suelto en el cuerpo, M6).</param>
    /// <response code="200">Venta anulada.</response>
    /// <response code="400">Ya anulada o con abonos.</response>
    /// <response code="404">Venta inexistente.</response>
    [HttpPost("{id}/anular")]
    [Authorize(Policy = PermisosConst.VentasDocumentosAnular)]
    [ProducesResponseType(typeof(Venta), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AnularVenta(int id, [FromBody] AnularVentaDto dto)
    {
        return Ok(await _ventasService.AnularVentaAsync(id, dto.Motivo, User.ObtenerUsuario()));
    }

    /// <summary>Registra un abono a una venta a crédito.</summary>
    /// <response code="200">Pago registrado con su asiento.</response>
    /// <response code="400">Monto mayor al saldo o venta anulada.</response>
    [HttpPost("pagos")]
    [Authorize(Policy = PermisosConst.VentasPagosCrear)]
    [ProducesResponseType(typeof(PagoVenta), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrarPago([FromBody] RegistrarPagoVentaDto dto)
    {
        return Ok(await _ventasService.RegistrarPagoAsync(dto, User.ObtenerUsuario()));
    }

    /// <summary>
    /// Catálogo para la caja: productos activos con precio y stock disponible (sin costos).
    /// Permite al Cajero vender sin acceso al módulo de Inventario.
    /// </summary>
    /// <response code="200">Productos vendibles.</response>
    [HttpGet("catalogo")]
    [Authorize(Policy = PermisosConst.VentasProductosConsultar)]
    [ProducesResponseType(typeof(List<ProductoCatalogoVentaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCatalogo([FromServices] ErpDbContext context)
    {
        var productos = await context.Productos
            .Where(p => p.Activo).OrderBy(p => p.Nombre)
            .Select(p => new ProductoCatalogoVentaDto(p.Id, p.Sku, p.Nombre, p.Categoria!.Nombre, p.PrecioVenta, p.StockActual, p.PuntoReorden))
            .ToListAsync();
        return Ok(productos);
    }

    /// <summary>Ventas con saldo pendiente (cobranzas), opcionalmente de un cliente, con días de mora.</summary>
    /// <response code="200">Ventas pendientes, de la más antigua a la más nueva.</response>
    [HttpGet("pendientes")]
    [Authorize(Policy = PermisosConst.VentasPagosCrear)]
    [ProducesResponseType(typeof(List<VentaPendienteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendientes([FromQuery] int? clienteId)
    {
        return Ok(await _ventasService.GetVentasPendientesAsync(clienteId));
    }

    /// <summary>Abono en cascada: reparte un monto entre las ventas pendientes del cliente (más antigua primero).</summary>
    /// <response code="200">Pagos aplicados y sobrante.</response>
    /// <response code="400">El cliente no tiene deuda o el método de pago es inválido.</response>
    [HttpPost("pagos/cascada")]
    [Authorize(Policy = PermisosConst.VentasPagosCrear)]
    [ProducesResponseType(typeof(AbonoCascadaResultadoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrarAbonoCascada([FromBody] AbonoCascadaDto dto)
    {
        return Ok(await _ventasService.RegistrarAbonoCascadaAsync(dto, User.ObtenerUsuario()));
    }

    /// <summary>Resumen de caja del usuario autenticado: ventas y abonos de hoy.</summary>
    /// <response code="200">Totales del día y lista de ventas propias.</response>
    [HttpGet("caja/hoy")]
    [Authorize(Policy = PermisosConst.VentasCajaPropias)]
    [ProducesResponseType(typeof(ResumenCajaDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCajaHoy()
    {
        return Ok(await _ventasService.GetResumenCajaAsync(User.ObtenerUsuario()));
    }
}
