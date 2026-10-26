// Módulo: Transversal (T6) — Responsable: Cristóbal
using ERP.Application.Interfaces;
using ERP.Domain.Entities.Compras;
using ERP.Domain.Entities.Contabilidad;
using ERP.Domain.Entities.Inventario;
using ERP.Domain.Entities.Ventas;
using ERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Services.Comun;

/// <summary>
/// Numerador de documentos basado en "MAX dentro de la transacción" protegido por un bloqueo de
/// aplicación de SQL Server (sp_getapplock). No requiere tabla nueva ni migración: el siguiente
/// número es el mayor existente del año + 1, considerando también los documentos aún no guardados
/// del contexto actual. El bloqueo se libera al confirmar/revertir la transacción, por lo que dos
/// peticiones concurrentes nunca obtienen el mismo número.
/// </summary>
public class NumeradorDocumentos : INumeradorDocumentos
{
    private readonly ErpDbContext _context;

    // Últimos números entregados en esta petición (por prefijo), aún sin guardar.
    private readonly Dictionary<string, int> _entregados = new();

    public NumeradorDocumentos(ErpDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<string> SiguienteAsync(TipoDocumentoNumerado tipo)
    {
        if (_context.Database.CurrentTransaction == null)
        {
            throw new InvalidOperationException("El numerador de documentos requiere una transacción activa.");
        }

        var prefijo = Prefijo(tipo);
        var baseNumero = $"{prefijo}-{DateTime.UtcNow.Year}-";

        // Bloqueo exclusivo por tipo de documento, ligado a la transacción actual.
        await _context.Database.ExecuteSqlRawAsync(
            @"DECLARE @r INT;
              EXEC @r = sp_getapplock @Resource = {0}, @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 15000;
              IF @r < 0 THROW 51000, 'No se pudo obtener el bloqueo de numeración de documentos.', 1;",
            "erp_numerador_" + prefijo);

        var ultimoBd = await ConsultaNumeros(tipo)
            .Where(n => n.StartsWith(baseNumero))
            .OrderByDescending(n => n)
            .FirstOrDefaultAsync();

        var maximo = new[] { Correlativo(ultimoBd, baseNumero) }
            .Concat(NumerosLocales(tipo).Select(n => Correlativo(n, baseNumero)))
            .Append(_entregados.GetValueOrDefault(baseNumero))
            .Max();

        var siguiente = maximo + 1;
        _entregados[baseNumero] = siguiente;
        return $"{baseNumero}{siguiente:D5}";
    }

    private static string Prefijo(TipoDocumentoNumerado tipo) => tipo switch
    {
        TipoDocumentoNumerado.OrdenCompra => "OC",
        TipoDocumentoNumerado.RecepcionCompra => "RC",
        TipoDocumentoNumerado.FacturaVenta => "FV",
        TipoDocumentoNumerado.AsientoContable => "AS",
        TipoDocumentoNumerado.AjusteInventario => "AJ",
        _ => throw new ArgumentOutOfRangeException(nameof(tipo))
    };

    private IQueryable<string> ConsultaNumeros(TipoDocumentoNumerado tipo) => tipo switch
    {
        TipoDocumentoNumerado.OrdenCompra => _context.OrdenesCompra.Select(x => x.NumeroOrden),
        TipoDocumentoNumerado.RecepcionCompra => _context.RecepcionesCompra.Select(x => x.NumeroRecepcion),
        TipoDocumentoNumerado.FacturaVenta => _context.Ventas.Select(x => x.NumeroFactura),
        TipoDocumentoNumerado.AsientoContable => _context.AsientosContables.Select(x => x.NumeroAsiento),
        TipoDocumentoNumerado.AjusteInventario => _context.AjustesInventario.Select(x => x.NumeroAjuste),
        _ => throw new ArgumentOutOfRangeException(nameof(tipo))
    };

    private IEnumerable<string> NumerosLocales(TipoDocumentoNumerado tipo) => tipo switch
    {
        TipoDocumentoNumerado.OrdenCompra => _context.OrdenesCompra.Local.Select(x => x.NumeroOrden),
        TipoDocumentoNumerado.RecepcionCompra => _context.RecepcionesCompra.Local.Select(x => x.NumeroRecepcion),
        TipoDocumentoNumerado.FacturaVenta => _context.Ventas.Local.Select(x => x.NumeroFactura),
        TipoDocumentoNumerado.AsientoContable => _context.AsientosContables.Local.Select(x => x.NumeroAsiento),
        TipoDocumentoNumerado.AjusteInventario => _context.AjustesInventario.Local.Select(x => x.NumeroAjuste),
        _ => Enumerable.Empty<string>()
    };

    /// <summary>Extrae el correlativo de "XX-AAAA-NNNNN"; 0 si no corresponde al año/prefijo.</summary>
    private static int Correlativo(string? numero, string baseNumero)
    {
        if (string.IsNullOrEmpty(numero) || !numero.StartsWith(baseNumero)) return 0;
        return int.TryParse(numero.AsSpan(baseNumero.Length), out var n) ? n : 0;
    }
}
