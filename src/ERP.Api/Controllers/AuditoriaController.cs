// Módulo: Seguridad / Auditoría (K4) — Responsable: Cristóbal Valdés
using ERP.Application.Common;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

/// <summary>
/// Consulta del registro de auditoría del sistema (RF-V03).
/// Solo accesible para usuarios con el permiso usuarios.administrar.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PermisosConst.UsuariosAdministrar)]
public class AuditoriaController : ControllerBase
{
    private readonly ErpDbContext _context;

    public AuditoriaController(ErpDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Consulta eventos de auditoría con filtros por fecha, usuario, módulo, acción y entidad.
    /// </summary>
    /// <param name="top">Cantidad máxima de registros a devolver (por defecto 200).</param>
    /// <param name="desde">Fecha inicial del rango.</param>
    /// <param name="hasta">Fecha final del rango.</param>
    /// <param name="usuario">Filtro por correo o fragmento de correo del usuario.</param>
    /// <param name="modulo">Filtro por módulo (Ventas, Compras, Inventario, Contabilidad, Seguridad).</param>
    /// <param name="accion">Filtro por tipo de acción (Creación, Modificación, Baja lógica, Eliminación, etc.).</param>
    /// <param name="entidad">Filtro por nombre de entidad afectada (Usuario, Rol, Venta, etc.).</param>
    /// <response code="200">Lista de eventos de auditoría ordenados cronológicamente descendente.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditorias(
        [FromQuery] int top = 200,
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null,
        [FromQuery] string? usuario = null,
        [FromQuery] string? modulo = null,
        [FromQuery] string? accion = null,
        [FromQuery] string? entidad = null)
    {
        var query = _context.Auditorias.AsNoTracking().AsQueryable();

        if (desde.HasValue)
            query = query.Where(a => a.FechaCreacion >= desde.Value.ToUniversalTime());

        if (hasta.HasValue)
            query = query.Where(a => a.FechaCreacion <= hasta.Value.ToUniversalTime());

        if (!string.IsNullOrWhiteSpace(usuario))
            query = query.Where(a => a.UsuarioEmail.Contains(usuario));

        if (!string.IsNullOrWhiteSpace(modulo))
            query = query.Where(a => a.Modulo == modulo);

        if (!string.IsNullOrWhiteSpace(accion))
            query = query.Where(a => a.Accion == accion);

        if (!string.IsNullOrWhiteSpace(entidad))
            query = query.Where(a => a.Entidad.Contains(entidad));

        var registros = await query
            .OrderByDescending(a => a.FechaCreacion)
            .Take(Math.Clamp(top, 1, 1000))
            .Select(a => new
            {
                a.Id,
                a.FechaCreacion,
                a.Modulo,
                a.Accion,
                a.Entidad,
                a.EntidadId,
                a.UsuarioEmail,
                a.IpAddress,
                a.Detalles
            })
            .ToListAsync();

        return Ok(registros);
    }
}