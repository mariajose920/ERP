// Módulo: Integración (configuración contable y monitoreo) — Responsable: Cristóbal
using ERP.Application.Common;
using ERP.Api.Infraestructura;
using ERP.Application.DTOs;
using ERP.Domain.Entities.Contabilidad;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

/// <summary>
/// Vista de la integración entre módulos: qué cuentas usa cada asiento automático, el estado de la
/// centralización y el registro de auditoría. La matriz de cuentas está fijada en código
/// (CuentasContablesConst); esta API la expone en solo lectura.
/// </summary>
[ApiController]
[Route("api/integracion")]
[Authorize]
public class IntegracionController : ControllerBase
{
    private readonly ErpDbContext _context;

    public IntegracionController(ErpDbContext context)
    {
        _context = context;
    }

    /// <summary>Matriz de centralización: operación, módulo y cuentas Debe/Haber (con su nombre en el plan).</summary>
    /// <response code="200">Operaciones y si sus cuentas existen en el plan de cuentas.</response>
    [HttpGet("configuracion")]
    [Authorize(Policy = PermisosConst.ContabilidadAsientosLeer)]
    public async Task<IActionResult> GetConfiguracion()
    {
        var operaciones = new[]
        {
            new { Operacion = "Factura de compra", Modulo = "Compras", Responsable = "Adán",
                  Debe = new[] { CuentasContablesConst.Inventario, CuentasContablesConst.IvaCreditoFiscal }, Haber = new[] { CuentasContablesConst.Proveedores } },
            new { Operacion = "Venta al contado (efectivo)", Modulo = "Ventas", Responsable = "María",
                  Debe = new[] { CuentasContablesConst.Caja }, Haber = new[] { CuentasContablesConst.IngresosVentas, CuentasContablesConst.IvaDebitoFiscal } },
            new { Operacion = "Venta con tarjeta / transferencia", Modulo = "Ventas", Responsable = "María",
                  Debe = new[] { CuentasContablesConst.Banco }, Haber = new[] { CuentasContablesConst.IngresosVentas, CuentasContablesConst.IvaDebitoFiscal } },
            new { Operacion = "Venta a crédito", Modulo = "Ventas", Responsable = "María",
                  Debe = new[] { CuentasContablesConst.Clientes }, Haber = new[] { CuentasContablesConst.IngresosVentas, CuentasContablesConst.IvaDebitoFiscal } },
            new { Operacion = "Costo de venta", Modulo = "Ventas", Responsable = "María",
                  Debe = new[] { CuentasContablesConst.CostoVentas }, Haber = new[] { CuentasContablesConst.Inventario } },
            new { Operacion = "Abono de cliente", Modulo = "Ventas", Responsable = "María",
                  Debe = new[] { CuentasContablesConst.Caja, CuentasContablesConst.Banco }, Haber = new[] { CuentasContablesConst.Clientes } },
            new { Operacion = "Ajuste de inventario (merma)", Modulo = "Inventario", Responsable = "Adán",
                  Debe = new[] { CuentasContablesConst.GastosMermas }, Haber = new[] { CuentasContablesConst.Inventario } },
            new { Operacion = "Ajuste de inventario (sobrante)", Modulo = "Inventario", Responsable = "Adán",
                  Debe = new[] { CuentasContablesConst.Inventario }, Haber = new[] { CuentasContablesConst.GastosMermas } }
        };

        var nombres = await _context.CuentasContables.AsNoTracking().ToDictionaryAsync(c => c.Codigo, c => c.Nombre);
        object Cuentas(string[] codigos) => codigos.Select(c => new { Codigo = c, Nombre = nombres.GetValueOrDefault(c), Existe = nombres.ContainsKey(c) });

        return Ok(new
        {
            TasaIva = ImpuestosConst.TasaIva,
            Operaciones = operaciones.Select(o => new { o.Operacion, o.Modulo, o.Responsable, Debe = Cuentas(o.Debe), Haber = Cuentas(o.Haber) })
        });
    }

    /// <summary>Estado de la centralización: asientos por módulo, descuadrados y último asiento.</summary>
    /// <response code="200">Indicadores de integración.</response>
    [HttpGet("estado")]
    [Authorize(Policy = PermisosConst.ContabilidadAsientosLeer)]
    public async Task<IActionResult> GetEstado()
    {
        var porModulo = await _context.AsientosContables.AsNoTracking()
            .GroupBy(a => a.ModuloOrigen)
            .Select(g => new { Modulo = g.Key, Cantidad = g.Count(), Ultimo = g.Max(a => a.Fecha) })
            .ToListAsync();

        return Ok(new
        {
            BaseDeDatos = await _context.Database.CanConnectAsync(),
            AsientosPorModulo = porModulo,
            AsientosDescuadrados = await _context.AsientosContables.CountAsync(a => a.TotalDebe != a.TotalHaber),
            VentasSinAsiento = await _context.Ventas.CountAsync(v => v.AsientoVentaId == null),
            FacturasCompraSinAsiento = await _context.FacturasCompra.CountAsync(f => f.AsientoContableId == null)
        });
    }

    /// <summary>Últimos registros de auditoría (requiere administrar usuarios).</summary>
    /// <param name="top">Cantidad (por defecto 100, máx. 500).</param>
    /// <response code="200">Eventos de auditoría, más reciente primero.</response>
    [HttpGet("auditoria")]
    [Authorize(Policy = PermisosConst.UsuariosAdministrar)]
    public async Task<IActionResult> GetAuditoria([FromQuery] int top = 100, [FromQuery] DateTime? desde = null, [FromQuery] DateTime? hasta = null,
        [FromQuery] string? usuario = null, [FromQuery] string? modulo = null, [FromQuery] string? entidad = null, [FromQuery] string? accion = null)
    {
        var query = _context.Auditorias.AsNoTracking().AsQueryable();
        if (desde.HasValue) query = query.Where(a => a.FechaCreacion >= desde.Value.ToUniversalTime());
        if (hasta.HasValue) query = query.Where(a => a.FechaCreacion <= hasta.Value.ToUniversalTime());
        if (!string.IsNullOrWhiteSpace(usuario)) query = query.Where(a => a.UsuarioEmail.Contains(usuario));
        if (!string.IsNullOrWhiteSpace(modulo)) query = query.Where(a => a.Modulo == modulo);
        if (!string.IsNullOrWhiteSpace(entidad)) query = query.Where(a => a.Entidad.Contains(entidad));
        if (!string.IsNullOrWhiteSpace(accion)) query = query.Where(a => a.Accion == accion);

        return Ok(await query
            .OrderByDescending(a => a.FechaCreacion)
            .Take(Math.Clamp(top, 1, 500))
            .Select(a => new { a.Id, a.FechaCreacion, a.Modulo, a.Accion, a.Entidad, a.EntidadId, a.UsuarioEmail, a.IpAddress, a.Detalles })
            .ToListAsync());
    }

    private IQueryable<MatrizIntegracionDto> ProyectarMatriz(IQueryable<MatrizIntegracionContable> query) =>
        query.Select(m => new MatrizIntegracionDto(
            m.Id, m.ModuloOrigen, m.TipoOperacion, m.Descripcion,
            m.CuentaDebeId, m.CuentaDebe!.Codigo, m.CuentaDebe.Nombre,
            m.CuentaHaberId, m.CuentaHaber!.Codigo, m.CuentaHaber.Nombre, m.Activo));

    /// <summary>Matriz de integración contable configurable (módulo + operación → cuenta Debe / Haber).</summary>
    /// <response code="200">Filas de la matriz con los datos de sus cuentas.</response>
    [HttpGet("matriz")]
    [Authorize(Policy = PermisosConst.ContabilidadAsientosLeer)]
    [ProducesResponseType(typeof(List<MatrizIntegracionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMatriz()
    {
        return Ok(await ProyectarMatriz(_context.MatrizIntegracionContable.AsNoTracking())
            .OrderBy(m => m.ModuloOrigen).ThenBy(m => m.TipoOperacion).ToListAsync());
    }

    private async Task ValidarCuentasMatrizAsync(int debeId, int haberId)
    {
        if (debeId == haberId)
            throw new InvalidOperationException("La cuenta del Debe y la del Haber deben ser distintas.");
        foreach (var id in new[] { debeId, haberId })
        {
            var cuenta = await _context.CuentasContables.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new InvalidOperationException($"La cuenta {id} no existe.");
            if (!cuenta.Activo || !cuenta.PermiteMovimiento)
                throw new InvalidOperationException($"La cuenta {cuenta.Codigo} - {cuenta.Nombre} está inactiva o no permite movimientos.");
        }
    }

    /// <summary>Agrega una operación a la matriz.</summary>
    /// <response code="200">Fila creada.</response>
    /// <response code="400">Cuentas inválidas u operación duplicada.</response>
    [HttpPost("matriz")]
    [Authorize(Policy = PermisosConst.ContabilidadCuentasEscribir)]
    [ProducesResponseType(typeof(MatrizIntegracionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearMatriz([FromBody] CrearMatrizIntegracionDto dto)
    {
        var modulo = dto.ModuloOrigen.Trim();
        var operacion = dto.TipoOperacion.Trim();
        if (await _context.MatrizIntegracionContable.AnyAsync(m => m.ModuloOrigen == modulo && m.TipoOperacion == operacion))
            throw new InvalidOperationException($"Ya existe la operación {modulo} / {operacion} en la matriz.");
        await ValidarCuentasMatrizAsync(dto.CuentaDebeId, dto.CuentaHaberId);

        var fila = new MatrizIntegracionContable
        {
            ModuloOrigen = modulo, TipoOperacion = operacion, Descripcion = dto.Descripcion?.Trim(),
            CuentaDebeId = dto.CuentaDebeId, CuentaHaberId = dto.CuentaHaberId, CreadoPor = User.ObtenerUsuario()
        };
        _context.MatrizIntegracionContable.Add(fila);
        await _context.SaveChangesAsync();
        return Ok(await ProyectarMatriz(_context.MatrizIntegracionContable.AsNoTracking().Where(m => m.Id == fila.Id)).FirstAsync());
    }

    /// <summary>Cambia las cuentas, la descripción o el estado de una fila de la matriz.</summary>
    /// <response code="200">Fila actualizada.</response>
    /// <response code="400">Cuentas inválidas.</response>
    /// <response code="404">Fila inexistente.</response>
    [HttpPut("matriz/{id:int}")]
    [Authorize(Policy = PermisosConst.ContabilidadCuentasEscribir)]
    [ProducesResponseType(typeof(MatrizIntegracionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActualizarMatriz(int id, [FromBody] ActualizarMatrizIntegracionDto dto)
    {
        var fila = await _context.MatrizIntegracionContable.FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException("Fila de la matriz no encontrada.");
        await ValidarCuentasMatrizAsync(dto.CuentaDebeId, dto.CuentaHaberId);

        fila.CuentaDebeId = dto.CuentaDebeId;
        fila.CuentaHaberId = dto.CuentaHaberId;
        fila.Descripcion = dto.Descripcion?.Trim();
        fila.Activo = dto.Activo;
        fila.ModificadoPor = User.ObtenerUsuario();
        fila.FechaModificacion = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(await ProyectarMatriz(_context.MatrizIntegracionContable.AsNoTracking().Where(m => m.Id == id)).FirstAsync());
    }
}
