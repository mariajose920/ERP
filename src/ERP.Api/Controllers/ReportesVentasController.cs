// Módulo: Ventas (reportes) — Responsable: María
using ERP.Application.Common;
using ERP.Domain.Enums;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

/// <summary>Reportes de ventas y cuentas por cobrar (ruta compartida /api/reportes).</summary>
[ApiController]
[Route("api/reportes")]
[Authorize]
public class ReportesVentasController : ControllerBase
{
    private readonly ErpDbContext _context;

    public ReportesVentasController(ErpDbContext context)
    {
        _context = context;
    }

    /// <summary>Total vendido por cliente (excluye anuladas) entre fechas opcionales.</summary>
    /// <response code="200">Una fila por cliente, de mayor a menor monto.</response>
    [HttpGet("ventas-por-cliente")]
    [Authorize(Policy = PermisosConst.VentasDocumentosLeer)]
    public async Task<IActionResult> GetVentasPorCliente([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        var query = _context.Ventas.Where(v => v.Estado != EstadoVenta.Anulada);

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

    /// <summary>
    /// Cuentas por cobrar con días de mora (fecha actual − FechaVencimiento; 0 si no ha vencido).
    /// Las ventas a crédito vencen a 30 días desde la emisión (M4).
    /// </summary>
    /// <response code="200">Ventas con saldo pendiente.</response>
    [HttpGet("cuentas-por-cobrar")]
    [Authorize(Policy = PermisosConst.VentasDocumentosLeer)]
    public async Task<IActionResult> GetCuentasPorCobrar()
    {
        var ahora = DateTime.UtcNow;
        var ventasPendientes = await _context.Ventas
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
                DiasMora = v.FechaVencimiento.HasValue && ahora > v.FechaVencimiento.Value
                    ? EF.Functions.DateDiffDay(v.FechaVencimiento.Value, ahora)
                    : 0
            })
            .OrderByDescending(v => v.SaldoPendiente)
            .ToListAsync();

        return Ok(ventasPendientes);
    }

    /// <summary>Ventas diarias (excluye anuladas) entre fechas; por defecto los últimos 30 días.</summary>
    /// <response code="200">Una fila por día con cantidad y total.</response>
    [HttpGet("ventas-por-dia")]
    [Authorize(Policy = PermisosConst.VentasDocumentosLeer)]
    public async Task<IActionResult> GetVentasPorDia([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        var d = (desde ?? DateTime.Today.AddDays(-29)).ToUniversalTime();
        var h = (hasta ?? DateTime.Today.AddDays(1)).ToUniversalTime();
        var filas = await _context.Ventas
            .Where(v => v.Estado != EstadoVenta.Anulada && v.FechaEmision >= d && v.FechaEmision < h)
            .GroupBy(v => v.FechaEmision.Date)
            .Select(g => new { Fecha = g.Key, CantidadVentas = g.Count(), TotalVendido = g.Sum(v => v.Total) })
            .OrderBy(x => x.Fecha)
            .ToListAsync();
        return Ok(filas);
    }

    /// <summary>Ranking de productos vendidos (unidades y monto neto) entre fechas opcionales.</summary>
    /// <response code="200">Productos de mayor a menor venta.</response>
    [HttpGet("ventas-por-producto")]
    [Authorize(Policy = PermisosConst.VentasDocumentosLeer)]
    public async Task<IActionResult> GetVentasPorProducto([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta, [FromQuery] int top = 20)
    {
        var query = _context.DetallesVenta.Where(d => d.Venta!.Estado != EstadoVenta.Anulada);
        if (desde.HasValue) query = query.Where(d => d.Venta!.FechaEmision >= desde.Value.ToUniversalTime());
        if (hasta.HasValue) query = query.Where(d => d.Venta!.FechaEmision <= hasta.Value.ToUniversalTime());

        var filas = await query
            .GroupBy(d => new { d.ProductoId, d.Producto!.Sku, d.Producto.Nombre })
            .Select(g => new
            {
                g.Key.ProductoId, g.Key.Sku, g.Key.Nombre,
                Unidades = g.Sum(x => x.Cantidad),
                TotalNeto = g.Sum(x => x.Subtotal),
                Margen = g.Sum(x => x.Subtotal - x.Cantidad * x.CostoUnitarioSnapshot)
            })
            .OrderByDescending(x => x.TotalNeto)
            .Take(Math.Clamp(top, 1, 200))
            .ToListAsync();
        return Ok(filas);
    }

    /// <summary>
    /// Comisiones sugeridas por vendedor (usuario que emitió la venta): porcentaje sobre el neto vendido
    /// (sin IVA, excluye anuladas) entre fechas opcionales. El porcentaje sale de Usuarios.PorcentajeComision
    /// de cada vendedor (2 % si no lo tiene configurado); el parámetro <c>porcentaje</c> es un override opcional
    /// que se aplica a todos.
    /// </summary>
    /// <param name="porcentaje">Override opcional del porcentaje de comisión (entre 0 y 100) para todos los vendedores.</param>
    /// <response code="200">Una fila por vendedor, de mayor a menor comisión.</response>
    [HttpGet("comisiones-ventas")]
    [Authorize(Policy = PermisosConst.VentasDocumentosLeer)]
    public async Task<IActionResult> GetComisionesVentas([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta, [FromQuery] decimal? porcentaje = null)
    {
        if (porcentaje.HasValue) porcentaje = Math.Clamp(porcentaje.Value, 0m, 100m);
        var query = _context.Ventas.Where(v => v.Estado != EstadoVenta.Anulada);
        if (desde.HasValue) query = query.Where(v => v.FechaEmision >= desde.Value.ToUniversalTime());
        if (hasta.HasValue) query = query.Where(v => v.FechaEmision <= hasta.Value.ToUniversalTime());

        var filas = await query
            .GroupBy(v => v.CreadoPor ?? "(sin usuario)")
            .Select(g => new
            {
                Vendedor = g.Key,
                CantidadVentas = g.Count(),
                TotalNeto = g.Sum(v => v.Subtotal),
                TotalConIva = g.Sum(v => v.Total)
            })
            .ToListAsync();

        // CreadoPor guarda el usuario (email/login) que emitió la venta: de ahí se obtiene su % configurado.
        var porcentajes = await _context.Usuarios.AsNoTracking()
            .Where(u => u.PorcentajeComision != null)
            .ToDictionaryAsync(u => u.Email.ToLower(), u => u.PorcentajeComision!.Value);

        return Ok(filas
            .Select(f =>
            {
                var propio = porcentajes.TryGetValue(f.Vendedor.ToLower(), out var p) ? p : (decimal?)null;
                var aplicado = porcentaje ?? propio ?? ComisionesConst.PorcentajePorDefecto;
                return new
                {
                    f.Vendedor, f.CantidadVentas, f.TotalNeto, f.TotalConIva,
                    Porcentaje = aplicado,
                    Origen = porcentaje.HasValue ? "override" : propio.HasValue ? "usuario" : "defecto",
                    ComisionSugerida = Math.Round(f.TotalNeto * aplicado / 100m, 0)
                };
            })
            .OrderByDescending(f => f.ComisionSugerida));
    }

    /// <summary>KPI de ventas: hoy, mes en curso, ticket promedio, cuentas por cobrar y vencidas.</summary>
    /// <response code="200">Indicadores.</response>
    [HttpGet("resumen-ventas")]
    [Authorize(Policy = PermisosConst.VentasDocumentosLeer)]
    public async Task<IActionResult> GetResumenVentas()
    {
        var hoy = DateTime.Today.ToUniversalTime();
        var inicioMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).ToUniversalTime();
        var ahora = DateTime.UtcNow;
        var vigentes = _context.Ventas.Where(v => v.Estado != EstadoVenta.Anulada);

        var ventasHoy = await vigentes.Where(v => v.FechaEmision >= hoy).SumAsync(v => (decimal?)v.Total) ?? 0;
        var mes = await vigentes.Where(v => v.FechaEmision >= inicioMes)
            .GroupBy(_ => 1).Select(g => new { Total = g.Sum(v => v.Total), Cantidad = g.Count() }).FirstOrDefaultAsync();
        var cxc = await vigentes.SumAsync(v => (decimal?)v.SaldoPendiente) ?? 0;
        var vencido = await vigentes.Where(v => v.SaldoPendiente > 0 && v.FechaVencimiento < ahora).SumAsync(v => (decimal?)v.SaldoPendiente) ?? 0;

        return Ok(new
        {
            VentasHoy = ventasHoy,
            VentasMes = mes?.Total ?? 0,
            CantidadVentasMes = mes?.Cantidad ?? 0,
            TicketPromedioMes = mes == null || mes.Cantidad == 0 ? 0 : Math.Round(mes.Total / mes.Cantidad, 0),
            CuentasPorCobrar = cxc,
            CuentasVencidas = vencido
        });
    }
}
