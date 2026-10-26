// Módulo: Compras (maestro de proveedores, A2) — Responsable: Adán
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
/// CRUD de proveedores con DTOs y eliminación lógica. Ruta nueva: /api/proveedores.
/// Se mantiene /api/maestros/proveedores por compatibilidad con la colección Postman.
/// </summary>
[ApiController]
[Route("api/proveedores")]
[Route("api/maestros/proveedores")]
[Authorize]
public class ProveedoresController : ControllerBase
{
    private readonly ErpDbContext _context;

    public ProveedoresController(ErpDbContext context)
    {
        _context = context;
    }

    /// <summary>Lista proveedores (por defecto solo activos).</summary>
    /// <param name="incluirInactivos">true para incluir los dados de baja.</param>
    /// <response code="200">Proveedores.</response>
    [HttpGet]
    [Authorize(Policy = PermisosConst.ComprasProveedoresLeer)]
    [ProducesResponseType(typeof(List<ProveedorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] bool incluirInactivos = false)
    {
        var query = _context.Proveedores.AsNoTracking();
        if (!incluirInactivos) query = query.Where(p => p.Activo);
        return Ok(await query.OrderBy(p => p.RazonSocial).Select(p => ADto(p)).ToListAsync());
    }

    /// <summary>Obtiene un proveedor por Id.</summary>
    /// <response code="200">Proveedor.</response>
    /// <response code="404">No existe.</response>
    [HttpGet("{id:int}")]
    [Authorize(Policy = PermisosConst.ComprasProveedoresLeer)]
    [ProducesResponseType(typeof(ProveedorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obtener(int id)
    {
        var p = await _context.Proveedores.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException("Proveedor no encontrado.");
        return Ok(ADto(p));
    }

    /// <summary>Crea un proveedor.</summary>
    /// <response code="201">Proveedor creado.</response>
    /// <response code="400">Datos inválidos o RUT duplicado.</response>
    [HttpPost]
    [Authorize(Policy = PermisosConst.ComprasProveedoresEscribir)]
    [ProducesResponseType(typeof(ProveedorDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] GuardarProveedorDto dto)
    {
        await ValidarRutUnicoAsync(dto.Rut, null);
        var proveedor = new Proveedor { CreadoPor = User.ObtenerUsuario() };
        Copiar(dto, proveedor);
        await _context.Proveedores.AddAsync(proveedor);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Obtener), new { id = proveedor.Id }, ADto(proveedor));
    }

    /// <summary>Actualiza un proveedor.</summary>
    /// <response code="200">Proveedor actualizado.</response>
    /// <response code="404">No existe.</response>
    [HttpPut("{id:int}")]
    [Authorize(Policy = PermisosConst.ComprasProveedoresEscribir)]
    [ProducesResponseType(typeof(ProveedorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(int id, [FromBody] GuardarProveedorDto dto)
    {
        var proveedor = await _context.Proveedores.FindAsync(id) ?? throw new KeyNotFoundException("Proveedor no encontrado.");
        await ValidarRutUnicoAsync(dto.Rut, id);
        Copiar(dto, proveedor);
        proveedor.FechaModificacion = DateTime.UtcNow;
        proveedor.ModificadoPor = User.ObtenerUsuario();
        await _context.SaveChangesAsync();
        return Ok(ADto(proveedor));
    }

    /// <summary>Da de baja (lógica) un proveedor.</summary>
    /// <response code="204">Proveedor desactivado.</response>
    /// <response code="400">Tiene órdenes de compra abiertas.</response>
    /// <response code="404">No existe.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = PermisosConst.ComprasProveedoresEscribir)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id)
    {
        var proveedor = await _context.Proveedores.FindAsync(id) ?? throw new KeyNotFoundException("Proveedor no encontrado.");
        var tieneAbiertas = await _context.OrdenesCompra.AnyAsync(o => o.ProveedorId == id &&
            (o.Estado == Domain.Enums.EstadoOrdenCompra.Pendiente || o.Estado == Domain.Enums.EstadoOrdenCompra.Aprobada || o.Estado == Domain.Enums.EstadoOrdenCompra.RecibidaParcial));
        if (tieneAbiertas)
            throw new InvalidOperationException("El proveedor tiene órdenes de compra abiertas; no puede darse de baja.");

        proveedor.Activo = false;
        proveedor.FechaModificacion = DateTime.UtcNow;
        proveedor.ModificadoPor = User.ObtenerUsuario();
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private async Task ValidarRutUnicoAsync(string rut, int? idActual)
    {
        var limpio = rut.Trim();
        if (await _context.Proveedores.AnyAsync(p => p.Rut == limpio && p.Id != idActual))
            throw new InvalidOperationException($"Ya existe un proveedor con RUT {limpio}.");
    }

    private static void Copiar(GuardarProveedorDto dto, Proveedor p)
    {
        p.Rut = dto.Rut.Trim();
        p.RazonSocial = dto.RazonSocial.Trim();
        p.Giro = dto.Giro;
        p.Contacto = dto.Contacto;
        p.Direccion = dto.Direccion;
        p.Telefono = dto.Telefono;
        p.Email = dto.Email.Trim();
        p.CondicionesPago = string.IsNullOrWhiteSpace(dto.CondicionesPago) ? "Contado" : dto.CondicionesPago;
    }

    private static ProveedorDto ADto(Proveedor p) =>
        new(p.Id, p.Rut, p.RazonSocial, p.Giro, p.Contacto, p.Direccion, p.Telefono, p.Email, p.CondicionesPago, p.Activo);
}
