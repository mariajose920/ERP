// Módulo: Ventas (maestro de clientes, M8) — Responsable: María
using ERP.Api.Infraestructura;
using ERP.Application.Common;
using ERP.Application.DTOs;
using ERP.Domain.Entities.Maestros;
using ERP.Domain.Enums;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

/// <summary>
/// CRUD de clientes con DTOs (sin overposting de SaldoPendiente). Ruta nueva: /api/clientes.
/// Se mantiene /api/maestros/clientes por compatibilidad con la colección Postman.
/// </summary>
[ApiController]
[Route("api/clientes")]
[Route("api/maestros/clientes")]
[Authorize]
public class ClientesController : ControllerBase
{
    private readonly ErpDbContext _context;

    public ClientesController(ErpDbContext context)
    {
        _context = context;
    }

    /// <summary>Lista clientes (por defecto solo activos).</summary>
    /// <param name="incluirInactivos">true para incluir los dados de baja.</param>
    /// <response code="200">Clientes.</response>
    [HttpGet]
    [Authorize(Policy = PermisosConst.VentasClientesLeer)]
    [ProducesResponseType(typeof(List<ClienteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] bool incluirInactivos = false)
    {
        var query = _context.Clientes.AsNoTracking();
        if (!incluirInactivos) query = query.Where(c => c.Activo);
        return Ok(await query.OrderBy(c => c.RazonSocial).Select(c => ADto(c)).ToListAsync());
    }

    /// <summary>Obtiene un cliente por Id.</summary>
    /// <response code="200">Cliente.</response>
    /// <response code="404">No existe.</response>
    [HttpGet("{id:int}")]
    [Authorize(Policy = PermisosConst.VentasClientesLeer)]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obtener(int id)
    {
        var c = await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException("Cliente no encontrado.");
        return Ok(ADto(c));
    }

    /// <summary>Crea un cliente (SaldoPendiente inicia en 0). Disponible para el Cajero (clientes básicos).</summary>
    /// <response code="201">Cliente creado.</response>
    /// <response code="400">Datos inválidos o RUT duplicado.</response>
    [HttpPost]
    [Authorize(Policy = PermisosConst.VentasClientesCrear)]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] GuardarClienteDto dto)
    {
        await ValidarRutUnicoAsync(dto.Rut, null);
        var cliente = new Cliente { CreadoPor = User.ObtenerUsuario() };
        Copiar(dto, cliente);
        await _context.Clientes.AddAsync(cliente);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Obtener), new { id = cliente.Id }, ADto(cliente));
    }

    /// <summary>Actualiza los datos de un cliente (no modifica SaldoPendiente).</summary>
    /// <response code="200">Cliente actualizado.</response>
    /// <response code="404">No existe.</response>
    [HttpPut("{id:int}")]
    [Authorize(Policy = PermisosConst.VentasClientesEscribir)]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(int id, [FromBody] GuardarClienteDto dto)
    {
        var cliente = await _context.Clientes.FindAsync(id) ?? throw new KeyNotFoundException("Cliente no encontrado.");
        await ValidarRutUnicoAsync(dto.Rut, id);
        Copiar(dto, cliente);
        cliente.FechaModificacion = DateTime.UtcNow;
        cliente.ModificadoPor = User.ObtenerUsuario();
        await _context.SaveChangesAsync();
        return Ok(ADto(cliente));
    }

    /// <summary>Da de baja (lógica) un cliente.</summary>
    /// <response code="204">Cliente desactivado.</response>
    /// <response code="400">Tiene saldo pendiente.</response>
    /// <response code="404">No existe.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = PermisosConst.VentasClientesEscribir)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id) ?? throw new KeyNotFoundException("Cliente no encontrado.");
        if (cliente.SaldoPendiente > 0 ||
            await _context.Ventas.AnyAsync(v => v.ClienteId == id && v.SaldoPendiente > 0 && v.Estado != EstadoVenta.Anulada))
            throw new InvalidOperationException("El cliente tiene saldo pendiente; no puede darse de baja.");

        cliente.Activo = false;
        cliente.FechaModificacion = DateTime.UtcNow;
        cliente.ModificadoPor = User.ObtenerUsuario();
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private async Task ValidarRutUnicoAsync(string rut, int? idActual)
    {
        var limpio = rut.Trim();
        if (await _context.Clientes.AnyAsync(c => c.Rut == limpio && c.Id != idActual))
            throw new InvalidOperationException($"Ya existe un cliente con RUT {limpio}.");
    }

    private static void Copiar(GuardarClienteDto dto, Cliente c)
    {
        c.Rut = dto.Rut.Trim();
        c.RazonSocial = dto.RazonSocial.Trim();
        c.Giro = dto.Giro;
        c.Contacto = dto.Contacto;
        c.Direccion = dto.Direccion;
        c.Telefono = dto.Telefono;
        c.Email = dto.Email.Trim();
        c.LimiteCredito = dto.LimiteCredito;
    }

    private static ClienteDto ADto(Cliente c) =>
        new(c.Id, c.Rut, c.RazonSocial, c.Giro, c.Contacto, c.Direccion, c.Telefono, c.Email, c.LimiteCredito, c.SaldoPendiente, c.Activo);
}
