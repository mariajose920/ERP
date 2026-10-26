// Módulo: Contabilidad — Responsable: Cristóbal
using ERP.Application.DTOs;
using ERP.Application.Interfaces;
using ERP.Domain.Entities.Contabilidad;
using ERP.Domain.Entities.Maestros;
using ERP.Domain.Enums;
using ERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Services;

/// <summary>
/// Contabilidad general. Toda creación de asientos y actualización de saldos pasa por aquí (K3),
/// tanto los asientos manuales como los automáticos de Compras, Ventas e Inventario.
/// </summary>
public class ContabilidadService : IContabilidadService
{
    private readonly ErpDbContext _context;
    private readonly INumeradorDocumentos _numerador;

    public ContabilidadService(ErpDbContext context, INumeradorDocumentos numerador)
    {
        _context = context;
        _numerador = numerador;
    }

    /// <summary>
    /// Aplica un movimiento al saldo de la cuenta según su naturaleza:
    /// deudora → saldo += Debe − Haber; acreedora → saldo += Haber − Debe.
    /// Es el único lugar del sistema que modifica <see cref="CuentaContable.SaldoActual"/>.
    /// </summary>
    public static void AplicarMovimiento(CuentaContable cuenta, decimal debe, decimal haber)
    {
        cuenta.SaldoActual += cuenta.Naturaleza == NaturalezaCuenta.Deudora
            ? debe - haber
            : haber - debe;
    }

    /// <inheritdoc />
    public Task<AsientoContable> CrearAsientoAsync(CrearAsientoManualDto dto, string usuario)
    {
        return _context.EnTransaccionAsync(async () =>
        {
            // K1: validaciones de un asiento manual.
            if (dto.Lineas == null || dto.Lineas.Count < 2)
                throw new InvalidOperationException("Un asiento requiere al menos 2 líneas.");

            foreach (var l in dto.Lineas)
            {
                if (l.Debe < 0 || l.Haber < 0)
                    throw new InvalidOperationException("Los montos de Debe y Haber no pueden ser negativos.");
                if ((l.Debe > 0) == (l.Haber > 0))
                    throw new InvalidOperationException("Cada línea debe tener monto solo en el Debe o solo en el Haber.");
            }

            var totalDebe = dto.Lineas.Sum(l => l.Debe);
            var totalHaber = dto.Lineas.Sum(l => l.Haber);
            if (totalDebe != totalHaber)
                throw new InvalidOperationException($"El asiento contable no está cuadrado. Total Debe: {totalDebe}, Total Haber: {totalHaber}");

            var fechaAsiento = dto.Fecha == default ? DateTime.UtcNow : dto.Fecha.ToUniversalTime();
            await ValidarPeriodoAbiertoAsync(fechaAsiento);

            var ids = dto.Lineas.Select(l => l.CuentaContableId).Distinct().ToList();
            var cuentas = await _context.CuentasContables.Where(c => ids.Contains(c.Id)).ToDictionaryAsync(c => c.Id);
            foreach (var id in ids)
            {
                if (!cuentas.TryGetValue(id, out var cuenta) || !cuenta.Activo)
                    throw new InvalidOperationException($"La cuenta contable {id} no existe o está inactiva.");
                if (!cuenta.PermiteMovimiento)
                    throw new InvalidOperationException($"La cuenta {cuenta.Codigo} - {cuenta.Nombre} no permite movimientos (es de agrupación).");
            }

            var asiento = new AsientoContable
            {
                NumeroAsiento = await _numerador.SiguienteAsync(TipoDocumentoNumerado.AsientoContable),
                Fecha = fechaAsiento,
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

            foreach (var l in dto.Lineas)
                AplicarMovimiento(cuentas[l.CuentaContableId], l.Debe, l.Haber);

            await _context.AsientosContables.AddAsync(asiento);
            await _context.SaveChangesAsync();
            return asiento;
        });
    }

    /// <inheritdoc />
    public Task<AsientoContable> RegistrarAsientoAutomaticoAsync(AsientoAutomaticoDto dto, string usuario)
    {
        return _context.EnTransaccionAsync(async () =>
        {
            var lineas = dto.Lineas.Where(l => l.Debe != 0 || l.Haber != 0).ToList();
            if (lineas.Count < 2)
                throw new InvalidOperationException($"El asiento automático '{dto.Glosa}' no tiene líneas suficientes.");

            var totalDebe = lineas.Sum(l => l.Debe);
            var totalHaber = lineas.Sum(l => l.Haber);
            if (totalDebe != totalHaber)
                throw new InvalidOperationException($"El asiento automático '{dto.Glosa}' no está cuadrado ({totalDebe} vs {totalHaber}).");

            await ValidarPeriodoAbiertoAsync(DateTime.UtcNow);

            var codigos = lineas.Select(l => l.CodigoCuenta).Distinct().ToList();
            var cuentas = await _context.CuentasContables.Where(c => codigos.Contains(c.Codigo)).ToDictionaryAsync(c => c.Codigo);
            var faltantes = codigos.Where(c => !cuentas.ContainsKey(c)).ToList();
            if (faltantes.Count > 0)
                throw new InvalidOperationException($"Faltan cuentas en el plan de cuentas: {string.Join(", ", faltantes)}.");

            var asiento = new AsientoContable
            {
                NumeroAsiento = await _numerador.SiguienteAsync(TipoDocumentoNumerado.AsientoContable),
                Fecha = DateTime.UtcNow,
                Glosa = dto.Glosa,
                ModuloOrigen = dto.ModuloOrigen,
                DocumentoOrigenTipo = dto.DocumentoOrigenTipo,
                DocumentoOrigenId = dto.DocumentoOrigenId,
                TotalDebe = totalDebe,
                TotalHaber = totalHaber,
                CreadoPor = usuario,
                Detalles = lineas.Select(l => new DetalleAsientoContable
                {
                    CuentaContableId = cuentas[l.CodigoCuenta].Id,
                    Debe = l.Debe,
                    Haber = l.Haber,
                    GlosaLinea = l.GlosaLinea,
                    CreadoPor = usuario
                }).ToList()
            };

            foreach (var l in lineas)
                AplicarMovimiento(cuentas[l.CodigoCuenta], l.Debe, l.Haber);

            await _context.AsientosContables.AddAsync(asiento);
            await _context.SaveChangesAsync();
            return asiento;
        });
    }

    /// <inheritdoc />
    public Task<AsientoContable> RevertirAsientoAsync(int asientoId, string glosa, string usuario)
    {
        return _context.EnTransaccionAsync(async () =>
        {
            var original = await _context.AsientosContables
                .Include(a => a.Detalles).ThenInclude(d => d.CuentaContable)
                .FirstOrDefaultAsync(a => a.Id == asientoId)
                ?? throw new KeyNotFoundException($"Asiento {asientoId} no encontrado.");

            await ValidarPeriodoAbiertoAsync(DateTime.UtcNow);
            if (await _context.AsientosContables.AnyAsync(a => a.AsientoRevertidoId == asientoId))
                throw new InvalidOperationException($"El asiento {original.NumeroAsiento} ya fue revertido.");

            var reversa = new AsientoContable
            {
                NumeroAsiento = await _numerador.SiguienteAsync(TipoDocumentoNumerado.AsientoContable),
                Fecha = DateTime.UtcNow,
                Glosa = glosa,
                ModuloOrigen = original.ModuloOrigen,
                DocumentoOrigenTipo = "Reversa" + original.DocumentoOrigenTipo,
                DocumentoOrigenId = original.DocumentoOrigenId,
                TotalDebe = original.TotalHaber,
                TotalHaber = original.TotalDebe,
                AsientoRevertidoId = original.Id,
                CreadoPor = usuario,
                Detalles = original.Detalles.Select(d => new DetalleAsientoContable
                {
                    CuentaContableId = d.CuentaContableId,
                    Debe = d.Haber,
                    Haber = d.Debe,
                    GlosaLinea = $"Reversa: {d.GlosaLinea}",
                    Referencia = original.NumeroAsiento,
                    CreadoPor = usuario
                }).ToList()
            };

            foreach (var d in original.Detalles)
                AplicarMovimiento(d.CuentaContable!, d.Haber, d.Debe);

            await _context.AsientosContables.AddAsync(reversa);
            await _context.SaveChangesAsync();
            return reversa;
        });
    }

    /// <inheritdoc />
    public async Task<List<AsientoContable>> GetLibroDiarioAsync(DateTime? desde, DateTime? hasta)
    {
        var query = _context.AsientosContables
            .Include(a => a.Detalles)
                .ThenInclude(d => d.CuentaContable)
            .AsNoTracking()
            .AsQueryable();

        if (desde.HasValue) query = query.Where(a => a.Fecha >= desde.Value.ToUniversalTime());
        if (hasta.HasValue) query = query.Where(a => a.Fecha <= hasta.Value.ToUniversalTime());

        return await query.OrderByDescending(a => a.Fecha).ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<DetalleAsientoContable>> GetLibroMayorPorCuentaAsync(int cuentaId, DateTime? desde, DateTime? hasta)
    {
        var query = _context.DetallesAsientoContable
            .Include(d => d.AsientoContable)
            .Include(d => d.CuentaContable)
            .AsNoTracking()
            .Where(d => d.CuentaContableId == cuentaId);

        if (desde.HasValue) query = query.Where(d => d.AsientoContable!.Fecha >= desde.Value.ToUniversalTime());
        if (hasta.HasValue) query = query.Where(d => d.AsientoContable!.Fecha <= hasta.Value.ToUniversalTime());

        return await query.OrderBy(d => d.AsientoContable!.Fecha).ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<BalanceComprobacionFilaDto>> GetBalanceComprobacionAsync()
    {
        // K2: las sumas se agrupan en SQL (una fila por cuenta) en lugar de cargar todos los detalles.
        var sumas = await _context.DetallesAsientoContable
            .GroupBy(d => d.CuentaContableId)
            .Select(g => new { CuentaId = g.Key, Debe = g.Sum(x => x.Debe), Haber = g.Sum(x => x.Haber) })
            .ToDictionaryAsync(x => x.CuentaId);

        var cuentas = await _context.CuentasContables.AsNoTracking().OrderBy(c => c.Codigo).ToListAsync();

        return cuentas.Select(c =>
        {
            var debe = sumas.TryGetValue(c.Id, out var s) ? s.Debe : 0m;
            var haber = s?.Haber ?? 0m;

            // Saldo según naturaleza; si queda negativo se presenta en la columna contraria.
            var saldo = c.Naturaleza == NaturalezaCuenta.Deudora ? debe - haber : haber - debe;
            var deudor = c.Naturaleza == NaturalezaCuenta.Deudora ? Math.Max(saldo, 0) : Math.Max(-saldo, 0);
            var acreedor = c.Naturaleza == NaturalezaCuenta.Acreedora ? Math.Max(saldo, 0) : Math.Max(-saldo, 0);

            return new BalanceComprobacionFilaDto(c.Codigo, c.Nombre, debe, haber, deudor, acreedor);
        }).ToList();
    }

    /// <inheritdoc />
    public async Task<EstadoResultadosDto> GetEstadoResultadosAsync(DateTime? desde, DateTime? hasta)
    {
        var query = _context.DetallesAsientoContable.AsQueryable();

        if (desde.HasValue) query = query.Where(d => d.AsientoContable!.Fecha >= desde.Value.ToUniversalTime());
        if (hasta.HasValue) query = query.Where(d => d.AsientoContable!.Fecha <= hasta.Value.ToUniversalTime());

        // Agrupado en SQL por tipo de cuenta.
        var porTipo = await query
            .GroupBy(d => d.CuentaContable!.Tipo)
            .Select(g => new { Tipo = g.Key, Debe = g.Sum(x => x.Debe), Haber = g.Sum(x => x.Haber) })
            .ToListAsync();

        decimal Neto(TipoCuentaContable tipo, bool acreedora)
        {
            var fila = porTipo.FirstOrDefault(x => x.Tipo == tipo);
            if (fila == null) return 0;
            return acreedora ? fila.Haber - fila.Debe : fila.Debe - fila.Haber;
        }

        var ingresos = Neto(TipoCuentaContable.Ingreso, acreedora: true);
        var costo = Neto(TipoCuentaContable.Costo, acreedora: false);
        var margenBruto = ingresos - costo;
        var gastos = Neto(TipoCuentaContable.Gasto, acreedora: false);

        return new EstadoResultadosDto(ingresos, costo, margenBruto, gastos, margenBruto - gastos);
    }

    /// <inheritdoc />
    public async Task<BalanceGeneralDto> GetBalanceGeneralAsync()
    {
        // Los saldos ya están expresados según la naturaleza de cada cuenta (AplicarMovimiento).
        var cuentas = await _context.CuentasContables.AsNoTracking()
            .Where(c => c.Activo && c.SaldoActual != 0)
            .OrderBy(c => c.Codigo)
            .ToListAsync();

        decimal Total(TipoCuentaContable t) => cuentas.Where(c => c.Tipo == t).Sum(c => c.SaldoActual);

        var activos = Total(TipoCuentaContable.Activo);
        var pasivos = Total(TipoCuentaContable.Pasivo);
        var patrimonio = Total(TipoCuentaContable.Patrimonio);
        var resultado = Total(TipoCuentaContable.Ingreso) - Total(TipoCuentaContable.Costo) - Total(TipoCuentaContable.Gasto);

        var lineas = cuentas
            .Where(c => c.Tipo is TipoCuentaContable.Activo or TipoCuentaContable.Pasivo or TipoCuentaContable.Patrimonio)
            .Select(c => new LineaBalanceGeneralDto(c.Codigo, c.Nombre, c.Tipo, c.SaldoActual))
            .ToList();

        return new BalanceGeneralDto(activos, pasivos, patrimonio, resultado,
            Math.Abs(activos - (pasivos + patrimonio + resultado)) < 1m, lineas);
    }

    // ------------------------------------------------------------------
    // Períodos contables
    // ------------------------------------------------------------------

    /// <inheritdoc />
    public async Task ValidarPeriodoAbiertoAsync(DateTime fecha)
    {
        var f = fecha.Kind == DateTimeKind.Local ? fecha.ToUniversalTime() : fecha;
        var cerrado = await _context.PeriodosContables.AsNoTracking()
            .AnyAsync(p => p.Anio == f.Year && p.Mes == f.Month && p.Estado == EstadoPeriodoContable.Cerrado);
        if (cerrado)
            throw new InvalidOperationException($"El período contable {f.Month:00}/{f.Year} está cerrado; no se pueden registrar asientos en él.");
    }

    /// <inheritdoc />
    public async Task<List<PeriodoContableDto>> GetPeriodosAsync(int anio)
    {
        var existentes = await _context.PeriodosContables.AsNoTracking().Where(p => p.Anio == anio).ToListAsync();
        var inicioAnio = new DateTime(anio, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var finAnio = inicioAnio.AddYears(1);
        var cantidades = (await _context.AsientosContables.AsNoTracking()
                .Where(a => a.Fecha >= inicioAnio && a.Fecha < finAnio)
                .Select(a => a.Fecha.Month).ToListAsync())
            .GroupBy(m => m).ToDictionary(g => g.Key, g => g.Count());

        return Enumerable.Range(1, 12).Select(mes =>
        {
            var p = existentes.FirstOrDefault(x => x.Mes == mes);
            var (ini, fin) = RangoMes(anio, mes);
            return new PeriodoContableDto(p?.Id ?? 0, anio, mes, p?.FechaInicio ?? ini, p?.FechaFin ?? fin,
                (p?.Estado ?? EstadoPeriodoContable.Abierto).ToString(), p?.FechaCierre, p?.CerradoPor,
                cantidades.GetValueOrDefault(mes));
        }).ToList();
    }

    /// <inheritdoc />
    public async Task<PeriodoContable> CerrarPeriodoAsync(int anio, int mes, string usuario)
    {
        var periodo = await ObtenerOCrearPeriodoAsync(anio, mes, usuario);
        if (periodo.Estado == EstadoPeriodoContable.Cerrado)
            throw new InvalidOperationException($"El período {mes:00}/{anio} ya está cerrado.");

        periodo.Estado = EstadoPeriodoContable.Cerrado;
        periodo.FechaCierre = DateTime.UtcNow;
        periodo.CerradoPor = usuario;
        periodo.ModificadoPor = usuario;
        periodo.FechaModificacion = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return periodo;
    }

    /// <inheritdoc />
    public async Task<PeriodoContable> ReabrirPeriodoAsync(int anio, int mes, string usuario)
    {
        var periodo = await _context.PeriodosContables.FirstOrDefaultAsync(p => p.Anio == anio && p.Mes == mes)
            ?? throw new InvalidOperationException($"El período {mes:00}/{anio} no está cerrado.");
        if (periodo.Estado != EstadoPeriodoContable.Cerrado)
            throw new InvalidOperationException($"El período {mes:00}/{anio} no está cerrado.");

        periodo.Estado = EstadoPeriodoContable.Abierto;
        periodo.FechaCierre = null;
        periodo.CerradoPor = null;
        periodo.ModificadoPor = usuario;
        periodo.FechaModificacion = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return periodo;
    }

    private async Task<PeriodoContable> ObtenerOCrearPeriodoAsync(int anio, int mes, string usuario)
    {
        if (mes < 1 || mes > 12 || anio < 2000 || anio > 2100)
            throw new InvalidOperationException("Año o mes inválido.");
        var periodo = await _context.PeriodosContables.FirstOrDefaultAsync(p => p.Anio == anio && p.Mes == mes);
        if (periodo != null) return periodo;

        var (ini, fin) = RangoMes(anio, mes);
        periodo = new PeriodoContable { Anio = anio, Mes = mes, FechaInicio = ini, FechaFin = fin, CreadoPor = usuario };
        await _context.PeriodosContables.AddAsync(periodo);
        return periodo;
    }

    private static (DateTime Inicio, DateTime Fin) RangoMes(int anio, int mes)
    {
        var ini = new DateTime(anio, mes, 1, 0, 0, 0, DateTimeKind.Utc);
        return (ini, ini.AddMonths(1).AddTicks(-1));
    }

    // ------------------------------------------------------------------
    // Matriz de integración contable
    // ------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<(string CodigoDebe, string CodigoHaber)> ResolverCuentasAsync(
        string modulo, string operacion, string codigoDebePorDefecto, string codigoHaberPorDefecto)
    {
        var fila = await _context.MatrizIntegracionContable.AsNoTracking()
            .Where(m => m.ModuloOrigen == modulo && m.TipoOperacion == operacion && m.Activo)
            .Select(m => new { Debe = m.CuentaDebe!.Codigo, Haber = m.CuentaHaber!.Codigo })
            .FirstOrDefaultAsync();
        return fila == null ? (codigoDebePorDefecto, codigoHaberPorDefecto) : (fila.Debe, fila.Haber);
    }
}
