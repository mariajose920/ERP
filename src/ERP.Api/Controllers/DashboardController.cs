// Módulo: Integración (portal / dashboard) — Responsable: Cristóbal
using ERP.Api.Infraestructura;
using ERP.Application.Common;
using ERP.Domain.Enums;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

/// <summary>
/// Indicadores del portal (index_erp). Cada bloque solo se calcula y devuelve si el usuario tiene
/// el permiso del módulo correspondiente; los bloques sin permiso vienen en null.
/// </summary>
[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ErpDbContext _context;

    public DashboardController(ErpDbContext context)
    {
        _context = context;
    }

    /// <summary>Resumen de Ventas, Compras, Inventario y Contabilidad según permisos.</summary>
    /// <response code="200">Bloques de indicadores (null los no permitidos).</response>
    [HttpGet("resumen")]
    public async Task<IActionResult> GetResumen()
    {
        var hoy = DateTime.Today.ToUniversalTime();
        var inicioMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).ToUniversalTime();
        var ahora = DateTime.UtcNow;

        object? ventas = null, compras = null, inventario = null, contabilidad = null;

        if (User.TienePermiso(PermisosConst.VentasDocumentosLeer))
        {
            var vigentes = _context.Ventas.Where(v => v.Estado != EstadoVenta.Anulada);
            ventas = new
            {
                VentasHoy = await vigentes.Where(v => v.FechaEmision >= hoy).SumAsync(v => (decimal?)v.Total) ?? 0,
                VentasMes = await vigentes.Where(v => v.FechaEmision >= inicioMes).SumAsync(v => (decimal?)v.Total) ?? 0,
                CuentasPorCobrar = await vigentes.SumAsync(v => (decimal?)v.SaldoPendiente) ?? 0,
                DocumentosVencidos = await vigentes.CountAsync(v => v.SaldoPendiente > 0 && v.FechaVencimiento < ahora),
                UltimasVentas = await vigentes.OrderByDescending(v => v.FechaEmision).Take(5)
                    .Select(v => new { v.Id, v.NumeroFactura, v.FechaEmision, Cliente = v.Cliente!.RazonSocial, v.Total, Estado = v.Estado.ToString() })
                    .ToListAsync()
            };
        }

        if (User.TienePermiso(PermisosConst.ComprasOrdenesLeer))
        {
            compras = new
            {
                OrdenesAbiertas = await _context.OrdenesCompra.CountAsync(o => o.Estado == EstadoOrdenCompra.Pendiente || o.Estado == EstadoOrdenCompra.Aprobada || o.Estado == EstadoOrdenCompra.RecibidaParcial),
                ComprasMes = await _context.FacturasCompra.Where(f => f.Estado == EstadoFacturaCompra.Aprobada && f.FechaEmision >= inicioMes).SumAsync(f => (decimal?)f.Total) ?? 0,
                ProveedoresActivos = await _context.Proveedores.CountAsync(p => p.Activo)
            };
        }

        if (User.TienePermiso(PermisosConst.InventarioStockLeer))
        {
            var activos = _context.Productos.Where(p => p.Activo);
            inventario = new
            {
                ValorInventario = await activos.SumAsync(p => (decimal?)(p.StockActual * p.CostoPromedioPonderado)) ?? 0,
                ProductosActivos = await activos.CountAsync(),
                AlertasReorden = await activos.CountAsync(p => p.StockActual <= p.PuntoReorden),
                ProductosCriticos = await activos.Where(p => p.StockActual <= p.PuntoReorden).OrderBy(p => p.StockActual).Take(5)
                    .Select(p => new { p.Id, p.Sku, p.Nombre, p.StockActual, p.PuntoReorden }).ToListAsync()
            };
        }

        if (User.TienePermiso(PermisosConst.ContabilidadReportesLeer))
        {
            contabilidad = new
            {
                AsientosMes = await _context.AsientosContables.CountAsync(a => a.Fecha >= inicioMes),
                AsientosDescuadrados = await _context.AsientosContables.CountAsync(a => a.TotalDebe != a.TotalHaber),
                Caja = await _context.CuentasContables.Where(c => c.Codigo == CuentasContablesConst.Caja).Select(c => c.SaldoActual).FirstOrDefaultAsync(),
                Banco = await _context.CuentasContables.Where(c => c.Codigo == CuentasContablesConst.Banco).Select(c => c.SaldoActual).FirstOrDefaultAsync()
            };
        }

        return Ok(new { Ventas = ventas, Compras = compras, Inventario = inventario, Contabilidad = contabilidad });
    }
}
