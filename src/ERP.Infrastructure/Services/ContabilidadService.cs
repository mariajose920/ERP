using ERP.Application.DTOs;
using ERP.Application.Interfaces;
using ERP.Domain.Entities.Contabilidad;
using ERP.Domain.Enums;
using ERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Services;

public class ContabilidadService : IContabilidadService
{
    private readonly ErpDbContext _context;

    public ContabilidadService(ErpDbContext context)
    {
        _context = context;
    }

    public async Task<AsientoContable> CrearAsientoAsync(CrearAsientoManualDto dto, string usuario)
    {
        var totalDebe = dto.Lineas.Sum(l => l.Debe);
        var totalHaber = dto.Lineas.Sum(l => l.Haber);

        if (totalDebe != totalHaber)
        {
            throw new InvalidOperationException($"El asiento contable no está cuadrado. Total Debe: {totalDebe}, Total Haber: {totalHaber}");
        }

        var count = await _context.AsientosContables.CountAsync() + 1;
        var numeroAsiento = $"AS-{DateTime.UtcNow.Year}-{count:D5}";

        var asiento = new AsientoContable
        {
            NumeroAsiento = numeroAsiento,
            Fecha = dto.Fecha,
            Glosa = dto.Glosa,
            ModuloOrigen = "Contabilidad",
            TotalDebe = totalDebe,
            TotalHaber = totalHaber,
            CreadoPor = usuario,
            Detalles = dto.Lineas.Select(l => new DetalleAsientoContable
            {
                CuentaContableId = l.CuentaContableId,
                Debe = l.Debe,
                Haber = l.Haber,
                GlosaLinea = l.GlosaLinea,
                CreadoPor = usuario
            }).ToList()
        };

        // Update balances of accounts
        foreach (var linea in dto.Lineas)
        {
            var cuenta = await _context.CuentasContables.FindAsync(linea.CuentaContableId);
            if (cuenta != null)
            {
                if (cuenta.Naturaleza == NaturalezaCuenta.Deudora)
                {
                    cuenta.SaldoActual += (linea.Debe - linea.Haber);
                }
                else
                {
                    cuenta.SaldoActual += (linea.Haber - linea.Debe);
                }
            }
        }

        await _context.AsientosContables.AddAsync(asiento);
        await _context.SaveChangesAsync();

        return asiento;
    }

    public async Task<List<AsientoContable>> GetLibroDiarioAsync(DateTime? desde, DateTime? hasta)
    {
        var query = _context.AsientosContables
            .Include(a => a.Detalles)
                .ThenInclude(d => d.CuentaContable)
            .AsQueryable();

        if (desde.HasValue) query = query.Where(a => a.Fecha >= desde.Value.ToUniversalTime());
        if (hasta.HasValue) query = query.Where(a => a.Fecha <= hasta.Value.ToUniversalTime());

        return await query.OrderByDescending(a => a.Fecha).ToListAsync();
    }

    public async Task<List<DetalleAsientoContable>> GetLibroMayorPorCuentaAsync(int cuentaId, DateTime? desde, DateTime? hasta)
    {
        var query = _context.DetallesAsientoContable
            .Include(d => d.AsientoContable)
            .Include(d => d.CuentaContable)
            .Where(d => d.CuentaContableId == cuentaId)
            .AsQueryable();

        if (desde.HasValue) query = query.Where(d => d.AsientoContable!.Fecha >= desde.Value.ToUniversalTime());
        if (hasta.HasValue) query = query.Where(d => d.AsientoContable!.Fecha <= hasta.Value.ToUniversalTime());

        return await query.OrderBy(d => d.AsientoContable!.Fecha).ToListAsync();
    }

    public async Task<List<BalanceComprobacionFilaDto>> GetBalanceComprobacionAsync()
    {
        var cuentas = await _context.CuentasContables.OrderBy(c => c.Codigo).ToListAsync();
        var detalles = await _context.DetallesAsientoContable.ToListAsync();

        var resultado = new List<BalanceComprobacionFilaDto>();

        foreach (var c in cuentas)
        {
            var movimientos = detalles.Where(d => d.CuentaContableId == c.Id).ToList();
            var debe = movimientos.Sum(m => m.Debe);
            var haber = movimientos.Sum(m => m.Haber);

            decimal saldoDeudor = 0;
            decimal saldoAcreedor = 0;

            if (c.Naturaleza == NaturalezaCuenta.Deudora)
            {
                var saldo = debe - haber;
                if (saldo >= 0) saldoDeudor = saldo;
                else saldoAcreedor = Math.Abs(saldo);
            }
            else
            {
                var saldo = haber - debe;
                if (saldo >= 0) saldoAcreedor = saldo;
                else saldoDeudor = Math.Abs(saldo);
            }

            resultado.Add(new BalanceComprobacionFilaDto(
                c.Codigo,
                c.Nombre,
                debe,
                haber,
                saldoDeudor,
                saldoAcreedor
            ));
        }

        return resultado;
    }

    public async Task<EstadoResultadosDto> GetEstadoResultadosAsync(DateTime? desde, DateTime? hasta)
    {
        var query = _context.DetallesAsientoContable
            .Include(d => d.CuentaContable)
            .Include(d => d.AsientoContable)
            .AsQueryable();

        if (desde.HasValue) query = query.Where(d => d.AsientoContable!.Fecha >= desde.Value.ToUniversalTime());
        if (hasta.HasValue) query = query.Where(d => d.AsientoContable!.Fecha <= hasta.Value.ToUniversalTime());

        var items = await query.ToListAsync();

        var ventasHaber = items.Where(i => i.CuentaContable?.Tipo == TipoCuentaContable.Ingreso).Sum(i => i.Haber - i.Debe);
        var costoDebe = items.Where(i => i.CuentaContable?.Tipo == TipoCuentaContable.Costo).Sum(i => i.Debe - i.Haber);
        var margenBruto = ventasHaber - costoDebe;
        var gastosDebe = items.Where(i => i.CuentaContable?.Tipo == TipoCuentaContable.Gasto).Sum(i => i.Debe - i.Haber);
        var utilidadNeta = margenBruto - gastosDebe;

        return new EstadoResultadosDto(ventasHaber, costoDebe, margenBruto, gastosDebe, utilidadNeta);
    }
}
