// Módulo: Inventario (catálogo de productos y categorías) — Responsable: Adán
using ERP.Api.Infraestructura;
using ERP.Application.Common;
using ERP.Application.DTOs;
using ERP.Domain.Entities.Maestros;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

/// <summary>Productos y categorías (se conservan las rutas históricas /api/maestros/...).</summary>
[ApiController]
[Route("api/maestros")]
[Authorize]
public class CatalogoController : ControllerBase
{
    private readonly ErpDbContext _context;

    public CatalogoController(ErpDbContext context)
    {
        _context = context;
    }

    /// <summary>Lista los productos activos con su categoría.</summary>
    /// <response code="200">Productos.</response>
    [HttpGet("productos")]
    [Authorize(Policy = PermisosConst.InventarioStockLeer)]
    [ProducesResponseType(typeof(List<Producto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductos()
    {
        return Ok(await _context.Productos.Include(p => p.Categoria).AsNoTracking().Where(p => p.Activo).ToListAsync());
    }

    /// <summary>
    /// Crea un producto. El stock inicia en 0 y el CPP en el costo base: el stock solo entra por
    /// facturas de compra o ajustes (evita el overposting de StockActual/CPP).
    /// </summary>
    /// <response code="200">Producto creado.</response>
    /// <response code="400">Categoría inexistente o SKU duplicado.</response>
    [HttpPost("productos")]
    [Authorize(Policy = PermisosConst.InventarioProductosEscribir)]
    [ProducesResponseType(typeof(Producto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearProducto([FromBody] CrearProductoDto dto)
    {
        if (!await _context.Categorias.AnyAsync(c => c.Id == dto.CategoriaId && c.Activo))
            throw new InvalidOperationException("La categoría no existe o está inactiva.");
        if (await _context.Productos.AnyAsync(p => p.Sku == dto.Sku.Trim()))
            throw new InvalidOperationException($"Ya existe un producto con SKU {dto.Sku}.");

        var producto = new Producto
        {
            Sku = dto.Sku.Trim(),
            Nombre = dto.Nombre.Trim(),
            Descripcion = dto.Descripcion,
            CategoriaId = dto.CategoriaId,
            UnidadMedida = string.IsNullOrWhiteSpace(dto.UnidadMedida) ? "UN" : dto.UnidadMedida,
            PrecioVenta = dto.PrecioVenta,
            CostoBase = dto.CostoBase,
            CostoPromedioPonderado = dto.CostoBase,
            StockMinimo = dto.StockMinimo,
            PuntoReorden = dto.PuntoReorden,
            CreadoPor = User.ObtenerUsuario()
        };
        await _context.Productos.AddAsync(producto);
        await _context.SaveChangesAsync();
        return Ok(producto);
    }

    /// <summary>Lista las categorías activas.</summary>
    /// <response code="200">Categorías.</response>
    [HttpGet("categorias")]
    [Authorize(Policy = PermisosConst.InventarioStockLeer)]
    [ProducesResponseType(typeof(List<Categoria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategorias()
    {
        return Ok(await _context.Categorias.AsNoTracking().Where(c => c.Activo).ToListAsync());
    }

    /// <summary>Crea una categoría.</summary>
    /// <response code="200">Categoría creada.</response>
    [HttpPost("categorias")]
    [Authorize(Policy = PermisosConst.InventarioProductosEscribir)]
    [ProducesResponseType(typeof(Categoria), StatusCodes.Status200OK)]
    public async Task<IActionResult> CrearCategoria([FromBody] CrearCategoriaDto dto)
    {
        var categoria = new Categoria
        {
            Codigo = dto.Codigo.Trim(),
            Nombre = dto.Nombre.Trim(),
            Descripcion = dto.Descripcion,
            CreadoPor = User.ObtenerUsuario()
        };
        await _context.Categorias.AddAsync(categoria);
        await _context.SaveChangesAsync();
        return Ok(categoria);
    }

    /// <summary>Actualiza los datos comerciales de un producto (no modifica stock ni CPP).</summary>
    /// <response code="200">Producto actualizado.</response>
    /// <response code="400">Categoría inexistente o SKU duplicado.</response>
    /// <response code="404">Producto inexistente.</response>
    [HttpPut("productos/{id:int}")]
    [Authorize(Policy = PermisosConst.InventarioProductosEscribir)]
    [ProducesResponseType(typeof(Producto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActualizarProducto(int id, [FromBody] CrearProductoDto dto)
    {
        var producto = await _context.Productos.FindAsync(id) ?? throw new KeyNotFoundException("Producto no encontrado.");
        if (!await _context.Categorias.AnyAsync(c => c.Id == dto.CategoriaId && c.Activo))
            throw new InvalidOperationException("La categoría no existe o está inactiva.");
        if (await _context.Productos.AnyAsync(p => p.Sku == dto.Sku.Trim() && p.Id != id))
            throw new InvalidOperationException($"Ya existe otro producto con SKU {dto.Sku}.");

        producto.Sku = dto.Sku.Trim();
        producto.Nombre = dto.Nombre.Trim();
        producto.Descripcion = dto.Descripcion;
        producto.CategoriaId = dto.CategoriaId;
        producto.UnidadMedida = string.IsNullOrWhiteSpace(dto.UnidadMedida) ? "UN" : dto.UnidadMedida;
        producto.PrecioVenta = dto.PrecioVenta;
        producto.CostoBase = dto.CostoBase;
        producto.StockMinimo = dto.StockMinimo;
        producto.PuntoReorden = dto.PuntoReorden;
        producto.ModificadoPor = User.ObtenerUsuario();
        producto.FechaModificacion = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(producto);
    }

    /// <summary>Da de baja (lógica) un producto sin stock.</summary>
    /// <response code="204">Producto desactivado.</response>
    /// <response code="400">El producto aún tiene stock.</response>
    /// <response code="404">Producto inexistente.</response>
    [HttpDelete("productos/{id:int}")]
    [Authorize(Policy = PermisosConst.InventarioProductosEscribir)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EliminarProducto(int id)
    {
        var producto = await _context.Productos.FindAsync(id) ?? throw new KeyNotFoundException("Producto no encontrado.");
        if (producto.StockActual > 0)
            throw new InvalidOperationException("No se puede dar de baja un producto con stock; ajuste el inventario primero.");
        producto.Activo = false;
        producto.ModificadoPor = User.ObtenerUsuario();
        producto.FechaModificacion = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
