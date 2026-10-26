// Módulo: Inventario — Responsable: Adán
using ERP.Application.Common;
using ERP.Application.DTOs;
using ERP.Application.Interfaces;
using ERP.Domain.Entities.Inventario;
using ERP.Domain.Entities.Maestros;
using ERP.Domain.Enums;
using ERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Services;

/// <summary>Stock, kardex, ajustes, alertas de reorden y valorización del inventario.</summary>
public class InventarioService : IInventarioService
{
    /// <summary>Tamaño máximo de página permitido para el kardex.</summary>
    public const int TamanoPaginaMaximo = 500;

    private readonly ErpDbContext _context;
    private readonly INumeradorDocumentos _numerador;
    private readonly IContabilidadService _contabilidad;

    public InventarioService(ErpDbContext context, INumeradorDocumentos numerador, IContabilidadService contabilidad)
    {
        _context = context;
        _numerador = numerador;
        _contabilidad = contabilidad;
    }

    /// <summary>
    /// Costo Promedio Ponderado (CPP) tras una entrada de mercadería (A4):
    /// <code>CPP = (StockAnterior × CPPAnterior + CantidadEntrada × CostoEntrada) / (StockAnterior + CantidadEntrada)</code>
    /// Regla: si el stock anterior es cero o negativo, el saldo anterior no aporta valor y el nuevo CPP
    /// es el costo de la entrada (evita CPP negativos o divisiones por cero). Si la entrada es 0, se
    /// conserva el CPP anterior.
    /// </summary>
    public static decimal CalcularCpp(decimal stockAnterior, decimal cppAnterior, decimal cantidadEntrada, decimal costoEntrada)
    {
        if (cantidadEntrada <= 0) return cppAnterior;
        if (stockAnterior <= 0) return costoEntrada;
        return Math.Round(
            (stockAnterior * cppAnterior + cantidadEntrada * costoEntrada) / (stockAnterior + cantidadEntrada),
            4);
    }

    /// <inheritdoc />
    public async Task<List<Producto>> GetStockActualAsync()
    {
        return await _context.Productos
            .Include(p => p.Categoria)
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<PaginaDto<MovimientoKardex>> GetKardexPorProductoAsync(int productoId, DateTime? desde, DateTime? hasta, int pagina, int tamanoPagina)
    {
        // A7: filtro por fechas y paginación.
        pagina = Math.Max(1, pagina);
        tamanoPagina = Math.Clamp(tamanoPagina, 1, TamanoPaginaMaximo);

        var query = _context.MovimientosKardex.AsNoTracking().Where(k => k.ProductoId == productoId);
        if (desde.HasValue) query = query.Where(k => k.Fecha >= desde.Value.ToUniversalTime());
        if (hasta.HasValue) query = query.Where(k => k.Fecha <= hasta.Value.ToUniversalTime());

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(k => k.Fecha).ThenByDescending(k => k.Id)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync();

        return new PaginaDto<MovimientoKardex>(items, pagina, tamanoPagina, total);
    }

    /// <inheritdoc />
    public Task<AjusteInventario> RegistrarAjusteAsync(RegistrarAjusteInventarioDto dto, string usuario)
    {
        return _context.EnTransaccionAsync(async () =>
        {
            if (dto.CantidadAjuste == 0)
                throw new InvalidOperationException("La cantidad del ajuste no puede ser 0.");

            var producto = await _context.Productos.FindAsync(dto.ProductoId)
                ?? throw new KeyNotFoundException("Producto no encontrado.");

            var stockAnterior = producto.StockActual;
            var nuevoStock = stockAnterior + dto.CantidadAjuste;

            if (nuevoStock < 0)
                throw new InvalidOperationException("El ajuste no puede dejar el stock en negativo.");

            var numeroAjuste = await _numerador.SiguienteAsync(TipoDocumentoNumerado.AjusteInventario);
            var costo = producto.CostoPromedioPonderado;
            var esEntrada = dto.CantidadAjuste > 0;
            var cantidadAbs = Math.Abs(dto.CantidadAjuste);

            var ajuste = new AjusteInventario
            {
                NumeroAjuste = numeroAjuste,
                Fecha = DateTime.UtcNow,
                ProductoId = producto.Id,
                CantidadAnterior = stockAnterior,
                CantidadAjuste = dto.CantidadAjuste,
                CantidadNueva = nuevoStock,
                CostoUnitario = costo,
                Motivo = dto.Motivo,
                CreadoPor = usuario
            };

            var kardex = new MovimientoKardex
            {
                ProductoId = producto.Id,
                Fecha = DateTime.UtcNow,
                TipoMovimiento = esEntrada ? TipoMovimientoKardex.AjusteEntrada : TipoMovimientoKardex.AjusteSalida,
                DocumentoTipo = "AjusteInventario",
                DocumentoNumero = numeroAjuste,
                CantidadEntrada = esEntrada ? cantidadAbs : 0,
                CostoUnitarioEntrada = esEntrada ? costo : 0,
                CantidadSalida = esEntrada ? 0 : cantidadAbs,
                CostoUnitarioSalida = esEntrada ? 0 : costo,
                SaldoCantidad = nuevoStock,
                CostoPromedioPonderadoResultante = costo,
                SaldoValorizado = nuevoStock * costo,
                Glosa = $"Ajuste por: {dto.Motivo}",
                CreadoPor = usuario
            };

            producto.StockActual = nuevoStock;

            await _context.AjustesInventario.AddAsync(ajuste);
            await _context.MovimientosKardex.AddAsync(kardex);
            await _context.SaveChangesAsync();

            kardex.DocumentoId = ajuste.Id;

            // A6: el ajuste impacta contabilidad (valorizado al CPP vigente).
            //   Salida (merma/pérdida): Debe Gastos/Mermas  / Haber Inventario
            //   Entrada (sobrante):     Debe Inventario     / Haber Gastos/Mermas (recupero)
            var monto = Math.Round(cantidadAbs * costo, 2);
            if (monto > 0)
            {
                var lineas = esEntrada
                    ? new List<LineaAsientoAutomaticoDto>
                    {
                        new(CuentasContablesConst.Inventario, monto, 0, "Ingreso por ajuste de inventario"),
                        new(CuentasContablesConst.GastosMermas, 0, monto, "Recupero por ajuste de inventario")
                    }
                    : new List<LineaAsientoAutomaticoDto>
                    {
                        new(CuentasContablesConst.GastosMermas, monto, 0, $"Merma/Pérdida: {dto.Motivo}"),
                        new(CuentasContablesConst.Inventario, 0, monto, "Salida por ajuste de inventario")
                    };

                var asiento = await _contabilidad.RegistrarAsientoAutomaticoAsync(new AsientoAutomaticoDto(
                    $"Ajuste de inventario {numeroAjuste} - {producto.Nombre}",
                    "Inventario", "AjusteInventario", ajuste.Id, lineas), usuario);

                ajuste.AsientoContableId = asiento.Id;
            }

            await _context.SaveChangesAsync();
            return ajuste;
        });
    }

    /// <inheritdoc />
    public async Task<List<Producto>> GetAlertasReordenAsync()
    {
        return await _context.Productos
            .Include(p => p.Categoria)
            .Where(p => p.Activo && p.StockActual <= p.PuntoReorden)
            .OrderBy(p => p.StockActual)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<decimal> GetValorizacionTotalInventarioAsync()
    {
        return await _context.Productos
            .Where(p => p.Activo)
            .SumAsync(p => p.StockActual * p.CostoPromedioPonderado);
    }

    /// <inheritdoc />
    public async Task<List<AjusteInventario>> GetAjustesAsync(int top)
    {
        return await _context.AjustesInventario
            .Include(a => a.Producto)
            .AsNoTracking()
            .OrderByDescending(a => a.Fecha)
            .Take(Math.Clamp(top, 1, 500))
            .ToListAsync();
    }
}
