using ERP.Domain.Enums;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportesController : ControllerBase
{
    private readonly ErpDbContext _context;

    public ReportesController(ErpDbContext context)
    {
        _context = context;
    }

    // 1. Reporte de Compras por Proveedor
    [HttpGet("compras-por-proveedor")]
    public async Task<IActionResult> GetComprasPorProveedor([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        var query = _context.FacturasCompra
            .Include(f => f.Proveedor)
            .Where(f => f.Estado == EstadoFacturaCompra.Aprobada)
            .AsQueryable();

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

    // 2. Reporte de Ventas por Cliente
    [HttpGet("ventas-por-cliente")]
    public async Task<IActionResult> GetVentasPorCliente([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        var query = _context.Ventas
            .Include(v => v.Cliente)
            .Where(v => v.Estado != EstadoVenta.Anulada)
            .AsQueryable();

        if (desde.HasValue) query = query.Where(v => v.FechaEmision >= desde.Value.ToUniversalTime());
        if (hasta.HasValue) query = query.Where(v => v.FechaEmision <= hasta.Value.ToUniversalTime());

        var result = await query
            .GroupBy(v => new { v.ClienteId, v.Cliente!.RazonSocial, v.Cliente.Rut })
            .Select(g => new
            {
                ClienteId = g.Key.ClienteId,
                g.Key.RazonSocial,
                g.Key.Rut,
                CantidadVentas = g.Count(),
                TotalVendido = g.Sum(v => v.Total)
            })
            .OrderByDescending(r => r.TotalVendido)
            .ToListAsync();

        return Ok(result);
    }

    // 3. Reporte de Cuentas por Cobrar
    [HttpGet("cuentas-por-cobrar")]
    public async Task<IActionResult> GetCuentasPorCobrar()
    {
        var ventasPendientes = await _context.Ventas
            .Include(v => v.Cliente)
            .Where(v => v.SaldoPendiente > 0 && v.Estado != EstadoVenta.Anulada)
            .Select(v => new
            {
                v.Id,
                v.NumeroFactura,
                v.FechaEmision,
                v.FechaVencimiento,
                Cliente = v.Cliente!.RazonSocial,
                ClienteRut = v.Cliente.Rut,
                v.Total,
                v.TotalPagado,
                v.SaldoPendiente,
                DiasMora = v.FechaVencimiento.HasValue && DateTime.UtcNow > v.FechaVencimiento.Value
                    ? (int)(DateTime.UtcNow - v.FechaVencimiento.Value).TotalDays
                    : 0
            })
            .OrderByDescending(v => v.SaldoPendiente)
            .ToListAsync();

        return Ok(ventasPendientes);
    }
}
