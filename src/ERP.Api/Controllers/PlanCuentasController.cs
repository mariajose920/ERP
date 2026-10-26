// Módulo: Contabilidad (plan de cuentas) — Responsable: Cristóbal
using ERP.Api.Infraestructura;
using ERP.Application.Common;
using ERP.Application.DTOs;
using ERP.Domain.Entities.Maestros;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

/// <summary>Plan de cuentas (se conserva la ruta histórica /api/maestros/cuentas-contables).</summary>
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

    /// <summary>Lista el plan de cuentas ordenado por código.</summary>
    /// <response code="200">Cuentas contables.</response>
    [HttpGet]
    [Authorize(Policy = PermisosConst.ContabilidadAsientosLeer)]
    [ProducesResponseType(typeof(List<CuentaContable>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCuentasContables()
    {
        return Ok(await _context.CuentasContables.AsNoTracking().OrderBy(c => c.Codigo).ToListAsync());
    }

    /// <summary>Crea una cuenta contable (el saldo inicia en 0 y solo cambia con asientos).</summary>
    /// <response code="200">Cuenta creada.</response>
    /// <response code="409">Código duplicado.</response>
    [HttpPost]
    [Authorize(Policy = PermisosConst.ContabilidadCuentasEscribir)]
    [ProducesResponseType(typeof(CuentaContable), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CrearCuentaContable([FromBody] CrearCuentaContableDto dto)
    {
        if (dto.CuentaPadreId.HasValue && !await _context.CuentasContables.AnyAsync(c => c.Id == dto.CuentaPadreId))
            throw new InvalidOperationException("La cuenta padre no existe.");

        var cuenta = new CuentaContable
        {
            Codigo = dto.Codigo.Trim(),
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

    /// <summary>Actualiza nombre, si permite movimientos y si está activa (código y naturaleza no cambian).</summary>
    /// <response code="200">Cuenta actualizada.</response>
    /// <response code="400">Se intenta desactivar una cuenta con saldo.</response>
    /// <response code="404">Cuenta inexistente.</response>
    [HttpPut("{id:int}")]
    [Authorize(Policy = PermisosConst.ContabilidadCuentasEscribir)]
    [ProducesResponseType(typeof(CuentaContable), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActualizarCuentaContable(int id, [FromBody] ActualizarCuentaContableDto dto)
    {
        var cuenta = await _context.CuentasContables.FindAsync(id) ?? throw new KeyNotFoundException("Cuenta no encontrada.");
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
}
