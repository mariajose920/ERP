using ERP.Domain.Entities.Maestros;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MaestrosController : ControllerBase
{
    private readonly ErpDbContext _context;

    public MaestrosController(ErpDbContext context)
    {
        _context = context;
    }

    // --- PRODUCTOS ---
    [HttpGet("productos")]
    public async Task<IActionResult> GetProductos()
    {
        var productos = await _context.Productos.Include(p => p.Categoria).Where(p => p.Activo).ToListAsync();
        return Ok(productos);
    }

    [HttpPost("productos")]
    public async Task<IActionResult> CrearProducto([FromBody] Producto producto)
    {
        producto.CreadoPor = User.Identity?.Name ?? "Sistema";
        await _context.Productos.AddAsync(producto);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProductos), new { id = producto.Id }, producto);
    }

    // --- CATEGORIAS ---
    [HttpGet("categorias")]
    public async Task<IActionResult> GetCategorias()
    {
        return Ok(await _context.Categorias.Where(c => c.Activo).ToListAsync());
    }

    [HttpPost("categorias")]
    public async Task<IActionResult> CrearCategoria([FromBody] Categoria categoria)
    {
        categoria.CreadoPor = User.Identity?.Name ?? "Sistema";
        await _context.Categorias.AddAsync(categoria);
        await _context.SaveChangesAsync();
        return Ok(categoria);
    }

    // --- PROVEEDORES (Adan) ---
    [HttpGet("proveedores")]
    public async Task<IActionResult> GetProveedores()
    {
        return Ok(await _context.Proveedores.Where(p => p.Activo).ToListAsync());
    }

    [HttpPost("proveedores")]
    public async Task<IActionResult> CrearProveedor([FromBody] Proveedor proveedor)
    {
        proveedor.CreadoPor = User.Identity?.Name ?? "Sistema";
        await _context.Proveedores.AddAsync(proveedor);
        await _context.SaveChangesAsync();
        return Ok(proveedor);
    }

    // --- CLIENTES (Maria) ---
    [HttpGet("clientes")]
    public async Task<IActionResult> GetClientes()
    {
        return Ok(await _context.Clientes.Where(c => c.Activo).ToListAsync());
    }

    [HttpPost("clientes")]
    public async Task<IActionResult> CrearCliente([FromBody] Cliente cliente)
    {
        cliente.CreadoPor = User.Identity?.Name ?? "Sistema";
        await _context.Clientes.AddAsync(cliente);
        await _context.SaveChangesAsync();
        return Ok(cliente);
    }

    // --- CUENTAS CONTABLES (Cristobal) ---
    [HttpGet("cuentas-contables")]
    public async Task<IActionResult> GetCuentasContables()
    {
        return Ok(await _context.CuentasContables.OrderBy(c => c.Codigo).ToListAsync());
    }

    [HttpPost("cuentas-contables")]
    public async Task<IActionResult> CrearCuentaContable([FromBody] CuentaContable cuenta)
    {
        cuenta.CreadoPor = User.Identity?.Name ?? "Sistema";
        await _context.CuentasContables.AddAsync(cuenta);
        await _context.SaveChangesAsync();
        return Ok(cuenta);
    }
}
