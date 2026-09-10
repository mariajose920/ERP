using ERP.Application.DTOs;
using ERP.Application.Interfaces;
using ERP.Domain.Entities.Contabilidad;
using ERP.Domain.Entities.Inventario;
using ERP.Domain.Entities.Maestros;
using ERP.Domain.Enums;
using ERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Services;

public class InventarioService : IInventarioService
{
    private readonly ErpDbContext _context;

    public InventarioService(ErpDbContext context)
    {
        _context = context;
    }

    public async Task<List<Producto>> GetStockActualAsync()
    {
        return await _context.Productos
            .Include(p => p.Categoria)
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task<List<MovimientoKardex>> GetKardexPorProductoAsync(int productoId)
    {
        return await _context.MovimientosKardex
            .Where(k => k.ProductoId == productoId)
            .OrderByDescending(k => k.Fecha)
            .ToListAsync();
    }

    public async Task<AjusteInventario> RegistrarAjusteAsync(RegistrarAjusteInventarioDto dto, string usuario)
    {
        using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            var producto = await _context.Productos.FindAsync(dto.ProductoId)
                ?? throw new InvalidOperationException("Producto no encontrado.");

            var stockAnterior = producto.StockActual;
            var nuevoStock = stockAnterior + dto.CantidadAjuste;

            if (nuevoStock < 0)
            {
                throw new InvalidOperationException("El ajuste no puede dejar el stock en negativo.");
            }

            var count = await _context.AjustesInventario.CountAsync() + 1;
            var numeroAjuste = $"AJ-{DateTime.UtcNow.Year}-{count:D5}";

            var ajuste = new AjusteInventario
            {
                NumeroAjuste = numeroAjuste,
                Fecha = DateTime.UtcNow,
                ProductoId = producto.Id,
                CantidadAnterior = stockAnterior,
                CantidadAjuste = dto.CantidadAjuste,
                CantidadNueva = nuevoStock,
                CostoUnitario = producto.CostoPromedioPonderado,
                Motivo = dto.Motivo,
                CreadoPor = usuario
            };

            // Kardex
            var tipoKardex = dto.CantidadAjuste > 0 ? TipoMovimientoKardex.AjusteEntrada : TipoMovimientoKardex.AjusteSalida;
            var cantidadAbs = Math.Abs(dto.CantidadAjuste);

            var kardex = new MovimientoKardex
            {
                ProductoId = producto.Id,
                Fecha = DateTime.UtcNow,
                TipoMovimiento = tipoKardex,
                DocumentoTipo = "AjusteInventario",
                DocumentoNumero = numeroAjuste,
                CantidadEntrada = dto.CantidadAjuste > 0 ? cantidadAbs : 0,
                CostoUnitarioEntrada = dto.CantidadAjuste > 0 ? producto.CostoPromedioPonderado : 0,
                CantidadSalida = dto.CantidadAjuste < 0 ? cantidadAbs : 0,
                CostoUnitarioSalida = dto.CantidadAjuste < 0 ? producto.CostoPromedioPonderado : 0,
                SaldoCantidad = nuevoStock,
                CostoPromedioPonderadoResultante = producto.CostoPromedioPonderado,
                SaldoValorizado = nuevoStock * producto.CostoPromedioPonderado,
                Glosa = $"Ajuste por: {dto.Motivo}",
                CreadoPor = usuario
            };

            producto.StockActual = nuevoStock;

            await _context.AjustesInventario.AddAsync(ajuste);
            await _context.MovimientosKardex.AddAsync(kardex);
            await _context.SaveChangesAsync();

            await tx.CommitAsync();
            return ajuste;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<List<Producto>> GetAlertasReordenAsync()
    {
        return await _context.Productos
            .Include(p => p.Categoria)
            .Where(p => p.Activo && p.StockActual <= p.PuntoReorden)
            .OrderBy(p => p.StockActual)
            .ToListAsync();
    }

    public async Task<decimal> GetValorizacionTotalInventarioAsync()
    {
        return await _context.Productos
            .Where(p => p.Activo)
            .SumAsync(p => p.StockActual * p.CostoPromedioPonderado);
    }
}
