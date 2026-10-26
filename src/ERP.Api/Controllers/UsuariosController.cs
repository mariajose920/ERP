// Módulo: Seguridad (administración de usuarios y roles) — Responsable: Cristóbal
using ERP.Api.Infraestructura;
using ERP.Application.Common;
using ERP.Application.DTOs;
using ERP.Domain.Entities.Seguridad;
using ERP.Infrastructure.Data;
using ERP.Infrastructure.Services.Seguridad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

/// <summary>
/// Administración de usuarios, roles y permisos (RBAC). Todo requiere el permiso usuarios.administrar.
/// El alta de usuarios sigue en POST /api/auth/register.
/// </summary>
[ApiController]
[Route("api")]
[Authorize(Policy = PermisosConst.UsuariosAdministrar)]
public class UsuariosController : ControllerBase
{
    private readonly ErpDbContext _context;

    public UsuariosController(ErpDbContext context)
    {
        _context = context;
    }

    /// <summary>Lista todos los usuarios con su rol.</summary>
    /// <response code="200">Usuarios.</response>
    [HttpGet("usuarios")]
    [ProducesResponseType(typeof(List<UsuarioDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarUsuarios()
    {
        var usuarios = await _context.Usuarios.AsNoTracking()
            .OrderBy(u => u.NombreCompleto)
            .Select(u => new UsuarioDto(u.Id, u.NombreCompleto, u.Email, u.Rut, u.RolId, u.Rol!.Nombre, u.Activo, u.UltimoAcceso, u.PorcentajeComision))
            .ToListAsync();
        return Ok(usuarios);
    }

    /// <summary>Actualiza nombre, RUT, rol, estado y (opcionalmente) la contraseña de un usuario.</summary>
    /// <response code="200">Usuario actualizado.</response>
    /// <response code="400">Rol inexistente o intento de desactivarse a sí mismo.</response>
    /// <response code="404">Usuario inexistente.</response>
    [HttpPut("usuarios/{id:int}")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActualizarUsuario(int id, [FromBody] ActualizarUsuarioDto dto)
    {
        var usuario = await _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");
        var rol = await _context.Roles.FindAsync(dto.RolId) ?? throw new InvalidOperationException("El rol no existe.");
        if (!dto.Activo && string.Equals(usuario.Email, User.ObtenerUsuario(), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("No puede desactivar su propio usuario.");

        usuario.NombreCompleto = dto.NombreCompleto.Trim();
        usuario.Rut = dto.Rut.Trim();
        usuario.RolId = rol.Id;
        usuario.Activo = dto.Activo;
        usuario.PorcentajeComision = dto.PorcentajeComision; // null = usa el valor por defecto (2 %)
        if (!string.IsNullOrWhiteSpace(dto.NuevaPassword))
            usuario.PasswordHash = HashContrasenas.Hashear(dto.NuevaPassword);
        usuario.ModificadoPor = User.ObtenerUsuario();
        usuario.FechaModificacion = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new UsuarioDto(usuario.Id, usuario.NombreCompleto, usuario.Email, usuario.Rut, rol.Id, rol.Nombre, usuario.Activo, usuario.UltimoAcceso, usuario.PorcentajeComision));
    }

    /// <summary>Lista los roles con sus permisos y cantidad de usuarios.</summary>
    /// <response code="200">Roles.</response>
    [HttpGet("roles")]
    [ProducesResponseType(typeof(List<RolDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarRoles()
    {
        var roles = await _context.Roles.AsNoTracking()
            .Where(r => r.Activo)
            .OrderBy(r => r.Id)
            .Select(r => new RolDto(r.Id, r.Nombre, r.Descripcion,
                r.RolPermisos.Select(rp => rp.Permiso.Codigo).OrderBy(c => c).ToList(),
                r.Usuarios.Count))
            .ToListAsync();
        return Ok(roles);
    }

    /// <summary>Catálogo de permisos disponibles.</summary>
    /// <response code="200">Permisos agrupables por módulo.</response>
    [HttpGet("permisos")]
    [ProducesResponseType(typeof(List<PermisoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPermisos()
    {
        return Ok(await _context.Permisos.AsNoTracking().OrderBy(p => p.Modulo).ThenBy(p => p.Codigo)
            .Select(p => new PermisoDto(p.Codigo, p.Modulo, p.Descripcion)).ToListAsync());
    }

    /// <summary>
    /// Reemplaza los permisos de un rol. El Administrador y el Cajero tienen permisos fijos
    /// (el seed los restablece), por lo que no se editan aquí.
    /// </summary>
    /// <response code="200">Rol actualizado.</response>
    /// <response code="400">Rol protegido o código de permiso inexistente.</response>
    /// <response code="404">Rol inexistente.</response>
    [HttpPut("roles/{id:int}/permisos")]
    [ProducesResponseType(typeof(RolDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActualizarPermisosRol(int id, [FromBody] ActualizarPermisosRolDto dto)
    {
        var rol = await _context.Roles.Include(r => r.RolPermisos).FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new KeyNotFoundException("Rol no encontrado.");
        if (rol.Nombre is PermisosConst.RolAdministrador or PermisosConst.RolCajero)
            throw new InvalidOperationException($"Los permisos del rol {rol.Nombre} son fijos.");

        var permisos = await _context.Permisos.Where(p => dto.Permisos.Contains(p.Codigo)).ToListAsync();
        var invalidos = dto.Permisos.Except(permisos.Select(p => p.Codigo)).ToList();
        if (invalidos.Count > 0)
            throw new InvalidOperationException($"Permisos inexistentes: {string.Join(", ", invalidos)}.");

        _context.RolPermisos.RemoveRange(rol.RolPermisos);
        foreach (var p in permisos)
            _context.RolPermisos.Add(new RolPermiso { RolId = rol.Id, PermisoId = p.Id });
        await _context.SaveChangesAsync();

        var cantidad = await _context.Usuarios.CountAsync(u => u.RolId == rol.Id);
        return Ok(new RolDto(rol.Id, rol.Nombre, rol.Descripcion, permisos.Select(p => p.Codigo).OrderBy(c => c).ToList(), cantidad));
    }
}
