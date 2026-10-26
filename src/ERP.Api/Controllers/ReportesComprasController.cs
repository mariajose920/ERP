// Módulo: Compras (reportes) — Responsable: Adán
using ERP.Application.Common;
using ERP.Domain.Enums;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

/// <summary>Reportes de compras (ruta compartida /api/reportes).</summary>
[ApiController]
[Route("api/reportes")]
[Authorize]
public class ReportesComprasController : ControllerBase
{
    private readonly ErpDbContext _context;

    public ReportesComprasController(ErpDbContext context)
    {
        _context = context;
    }

    /// <summary>Total comprado por proveedor (facturas aprobadas) entre fechas opcionales.</summary>
    /// <response code="200">Una fila por proveedor, de mayor a menor monto.</response>
    [HttpGet("compras-por-proveedor")]
    [Authorize(Policy = PermisosConst.ComprasOrdenesLeer)]
    public async Task<IActionResult> GetComprasPorProveedor([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        var query = _context.FacturasCompra
            .Where(f => f.Estado == EstadoFacturaCompra.Aprobada);

        if (desde.HasValue) query = query.Where(f => f.FechaEmision >= desde.Value.ToUniversalTime());
        if (hasta.HasValue) query = query.Where(f => f.FechaEmision <= hasta.Value.ToUniversalTime());

        var result = await query
            .GroupBy(f => new { f.ProveedorId, f.Proveedor!.RazonSocial, f.Proveedor.Rut })
            .Select(g => new
            {
                ProveedorId = g.Key.ProveedorId,
                g.Key.RazonSocial,
                g.Key.Rut,
                CantidadFacturas = g.Count(),
                TotalComprado = g.Sum(f => f.Total)
            })
            .OrderByDescending(r => r.TotalComprado)
            .ToListAsync();

        return Ok(result);
    }

    /// <summary>Historial de precios de compra por producto (líneas de facturas aprobadas), más reciente primero.</summary>
    /// <param name="productoId">Producto (opcional).</param>
    /// <response code="200">Una fila por línea facturada con fecha, proveedor, factura y precio neto.</response>
    [HttpGet("historial-precios-compra")]
    [Authorize(Policy = PermisosConst.ComprasOrdenesLeer)]
    public async Task<IActionResult> GetHistorialPreciosCompra([FromQuery] int? productoId, [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        var query = _context.DetallesFacturaCompra
            .Where(d => d.FacturaCompra!.Estado == EstadoFacturaCompra.Aprobada);
        if (productoId.HasValue) query = query.Where(d => d.ProductoId == productoId.Value);
        if (desde.HasValue) query = query.Where(d => d.FacturaCompra!.FechaEmision >= desde.Value.ToUniversalTime());
        if (hasta.HasValue) query = query.Where(d => d.FacturaCompra!.FechaEmision <= hasta.Value.ToUniversalTime());

        var filas = await query
            .OrderByDescending(d => d.FacturaCompra!.FechaEmision)
            .Take(1000)
            .Select(d => new
            {
                d.ProductoId,
                Sku = d.Producto!.Sku,
                Producto = d.Producto.Nombre,
                Fecha = d.FacturaCompra!.FechaEmision,
                NumeroFactura = d.FacturaCompra.NumeroFactura,
                Proveedor = d.FacturaCompra.Proveedor!.RazonSocial,
                d.Cantidad,
                d.PrecioUnitario
            })
            .ToListAsync();
        return Ok(filas);
    }

    /// <summary>Cumplimiento de órdenes de compra: pedido vs recibido vs facturado por orden.</summary>
    /// <response code="200">Una fila por orden (no anulada) con porcentajes de cumplimiento.</response>
    [HttpGet("cumplimiento-ordenes")]
    [Authorize(Policy = PermisosConst.ComprasOrdenesLeer)]
    public async Task<IActionResult> GetCumplimientoOrdenes([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta, [FromQuery] int? proveedorId)
    {
        var query = _context.OrdenesCompra.Where(o => o.Estado != EstadoOrdenCompra.Anulada);
        if (desde.HasValue) query = query.Where(o => o.FechaEmision >= desde.Value.ToUniversalTime());
        if (hasta.HasValue) query = query.Where(o => o.FechaEmision <= hasta.Value.ToUniversalTime());
        if (proveedorId.HasValue) query = query.Where(o => o.ProveedorId == proveedorId.Value);

        var ordenes = await query
            .OrderByDescending(o => o.FechaEmision)
            .Select(o => new
            {
                o.Id,
                o.NumeroOrden,
                o.FechaEmision,
                o.FechaEntregaEsperada,
                Proveedor = o.Proveedor!.RazonSocial,
                Estado = o.Estado.ToString(),
                o.Total,
                Pedido = o.Detalles.Sum(d => d.Cantidad),
                Recibido = o.Detalles.Sum(d => d.CantidadRecibida),
                Facturado = o.Facturas.Where(f => f.Estado == EstadoFacturaCompra.Aprobada)
                    .SelectMany(f => f.Detalles).Sum(d => (decimal?)d.Cantidad) ?? 0
            })
            .ToListAsync();

        var ahora = DateTime.UtcNow;
        return Ok(ordenes.Select(o => new
        {
            o.Id, o.NumeroOrden, o.FechaEmision, o.FechaEntregaEsperada, o.Proveedor, o.Estado, o.Total,
            o.Pedido, o.Recibido, o.Facturado,
            PorcentajeRecibido = o.Pedido == 0 ? 0 : Math.Round(o.Recibido / o.Pedido * 100, 1),
            PorcentajeFacturado = o.Pedido == 0 ? 0 : Math.Round(o.Facturado / o.Pedido * 100, 1),
            Atrasada = o.FechaEntregaEsperada.HasValue && o.FechaEntregaEsperada.Value < ahora && o.Recibido < o.Pedido
        }));
    }

    /// <summary>Consolidado de facturas de compra con filtros de fecha, proveedor y estado.</summary>
    /// <response code="200">Facturas (máx. 1000), más reciente primero.</response>
    [HttpGet("consolidado-facturas-compra")]
    [Authorize(Policy = PermisosConst.ComprasOrdenesLeer)]
    public async Task<IActionResult> GetConsolidadoFacturas([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta,
        [FromQuery] int? proveedorId, [FromQuery] string? estado)
    {
        var query = _context.FacturasCompra.AsQueryable();
        if (desde.HasValue) query = query.Where(f => f.FechaEmision >= desde.Value.ToUniversalTime());
        if (hasta.HasValue) query = query.Where(f => f.FechaEmision <= hasta.Value.ToUniversalTime());
        if (proveedorId.HasValue) query = query.Where(f => f.ProveedorId == proveedorId.Value);
        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<EstadoFacturaCompra>(estado, true, out var est))
            query = query.Where(f => f.Estado == est);

        var filas = await query
            .OrderByDescending(f => f.FechaEmision)
            .Take(1000)
            .Select(f => new
            {
                f.Id,
                f.NumeroFactura,
                f.FechaEmision,
                f.FechaVencimiento,
                Proveedor = f.Proveedor!.RazonSocial,
                ProveedorRut = f.Proveedor.Rut,
                NumeroOrden = f.OrdenCompra != null ? f.OrdenCompra.NumeroOrden : null,
                Estado = f.Estado.ToString(),
                f.Subtotal,
                f.ImpuestoIva,
                f.Total,
                Contabilizada = f.AsientoContableId != null
            })
            .ToListAsync();
        return Ok(filas);
    }

    /// <summary>
    /// Cuentas por pagar: facturas aprobadas con su vencimiento y días de mora.
    /// El sistema no registra pagos a proveedores, por lo que el saldo de cada factura es su total.
    /// </summary>
    /// <response code="200">Facturas aprobadas, las más vencidas primero.</response>
    [HttpGet("cuentas-por-pagar")]
    [Authorize(Policy = PermisosConst.ComprasOrdenesLeer)]
    public async Task<IActionResult> GetCuentasPorPagar()
    {
        var ahora = DateTime.UtcNow;
        var filas = await _context.FacturasCompra
            .Where(f => f.Estado == EstadoFacturaCompra.Aprobada)
            .Select(f => new
            {
                f.Id,
                f.NumeroFactura,
                f.FechaEmision,
                f.FechaVencimiento,
                Proveedor = f.Proveedor!.RazonSocial,
                ProveedorRut = f.Proveedor.Rut,
                CondicionesPago = f.Proveedor.CondicionesPago,
                f.Total,
                DiasMora = ahora > f.FechaVencimiento ? EF.Functions.DateDiffDay(f.FechaVencimiento, ahora) : 0
            })
            .OrderByDescending(f => f.DiasMora)
            .ThenBy(f => f.FechaVencimiento)
            .ToListAsync();
        return Ok(filas);
    }
}
