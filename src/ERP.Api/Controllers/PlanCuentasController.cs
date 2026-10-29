// Módulo: Contabilidad (plan de cuentas, RF-V04) — Responsable: Cristóbal Valdés
using ERP.Api.Infraestructura;
using ERP.Application.Common;
using ERP.Application.DTOs;
using ERP.Domain.Entities.Maestros;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

/// <summary>
/// Gestión del plan de cuentas contable en árbol jerárquico (RF-V04).
/// </summary>
[ApiController]
[Route("api/maestros/cuentas-contables")]
[Authorize]
public class PlanCuentasController : ControllerBase
{
    private readonly ErpDbContext _context;

    public PlanCuentasController(ErpDbContext context)
    {
        _context = context;
    }

    /// <summary>Lista todas las cuentas contables ordenadas por código.</summary>
    /// <response code="200">Listado de cuentas.</response>
    [HttpGet]
    [Authorize(Policy = PermisosConst.ContabilidadAsientosLeer)]
    [ProducesResponseType(typeof(List<CuentaContable>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCuentasContables()
    {
        return Ok(await _context.CuentasContables.AsNoTracking().OrderBy(c => c.Codigo).ToListAsync());
    }

    /// <summary>Crea una nueva cuenta contable en el árbol.</summary>
    /// <param name="dto">Datos de la cuenta a crear.</param>
    /// <response code="200">Cuenta creada exitosamente.</response>
    /// <response code="400">Código duplicado o cuenta padre inválida.</response>
    [HttpPost]
    [Authorize(Policy = PermisosConst.ContabilidadCuentasEscribir)]
    [ProducesResponseType(typeof(CuentaContable), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearCuentaContable([FromBody] CrearCuentaContableDto dto)
    {
        var codigo = dto.Codigo.Trim();
        if (await _context.CuentasContables.AnyAsync(c => c.Codigo == codigo))
            throw new InvalidOperationException($"Ya existe una cuenta con el código {codigo}.");

        if (dto.CuentaPadreId.HasValue && !await _context.CuentasContables.AnyAsync(c => c.Id == dto.CuentaPadreId))
            throw new InvalidOperationException("La cuenta padre no existe.");

        var cuenta = new CuentaContable
        {
            Codigo = codigo,
            Nombre = dto.Nombre.Trim(),
            Tipo = dto.Tipo,
            Naturaleza = dto.Naturaleza,
            Nivel = dto.Nivel,
            CuentaPadreId = dto.CuentaPadreId,
            PermiteMovimiento = dto.PermiteMovimiento,
            CreadoPor = User.ObtenerUsuario()
        };

        await _context.CuentasContables.AddAsync(cuenta);
        await _context.SaveChangesAsync();
        return Ok(cuenta);
    }

    /// <summary>Actualiza el nombre, si permite movimientos y su estado de activación.</summary>
    /// <param name="id">ID de la cuenta a modificar.</param>
    /// <param name="dto">Datos actualizados.</param>
    /// <response code="200">Cuenta actualizada.</response>
    /// <response code="400">No se puede desactivar una cuenta con saldo vigente.</response>
    /// <response code="404">Cuenta inexistente.</response>
    [HttpPut("{id:int}")]
    [Authorize(Policy = PermisosConst.ContabilidadCuentasEscribir)]
    [ProducesResponseType(typeof(CuentaContable), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActualizarCuentaContable(int id, [FromBody] ActualizarCuentaContableDto dto)
    {
        var cuenta = await _context.CuentasContables.FindAsync(id) 
            ?? throw new KeyNotFoundException("Cuenta no encontrada.");

        if (!dto.Activo && cuenta.SaldoActual != 0)
            throw new InvalidOperationException("No se puede desactivar una cuenta con saldo distinto de cero.");

        cuenta.Nombre = dto.Nombre.Trim();
        cuenta.PermiteMovimiento = dto.PermiteMovimiento;
        cuenta.Activo = dto.Activo;
        cuenta.ModificadoPor = User.ObtenerUsuario();
        cuenta.FechaModificacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(cuenta);
    }

    /// <summary>Elimina una cuenta contable si no tiene saldo, asientos ni amarres en la matriz.</summary>
    /// <param name="id">ID de la cuenta a eliminar.</param>
    /// <response code="200">Cuenta eliminada.</response>
    /// <response code="400">Posee saldo, asientos o está configurada en la matriz.</response>
    /// <response code="404">Cuenta inexistente.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = PermisosConst.ContabilidadCuentasEscribir)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EliminarCuentaContable(int id)
    {
        var cuenta = await _context.CuentasContables.FindAsync(id) 
            ?? throw new KeyNotFoundException("Cuenta contable no encontrada.");

        if (cuenta.SaldoActual != 0)
            throw new InvalidOperationException("No se puede eliminar una cuenta que posee saldo.");

        if (await _context.DetallesAsientoContable.AnyAsync(d => d.CuentaContableId == id))
            throw new InvalidOperationException("No se puede eliminar una cuenta con asientos contables registrados. Puede desactivarla.");

        if (await _context.MatrizIntegracionContable.AnyAsync(m => m.CuentaDebeId == id || m.CuentaHaberId == id))
            throw new InvalidOperationException("No se puede eliminar: la cuenta está enlazada en la matriz de integración contable.");

        _context.CuentasContables.Remove(cuenta);
        await _context.SaveChangesAsync();
        return Ok(new { mensaje = "Cuenta eliminada correctamente." });
    }
}