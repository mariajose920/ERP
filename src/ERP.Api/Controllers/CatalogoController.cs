// Módulo: Inventario (catálogo de productos y categorías) — Responsable: Adán
using ERP.Api.Infraestructura;
using ERP.Application.Common;
using ERP.Application.DTOs;
using ERP.Application.Interfaces;
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
    private readonly IInventarioService _inventarioService;
    private readonly IAuthorizationService _autorizacion;

    public CatalogoController(ErpDbContext context, IInventarioService inventarioService, IAuthorizationService autorizacion)
    {
        _context = context;
        _inventarioService = inventarioService;
        _autorizacion = autorizacion;
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
    /// Crea un producto con el CPP en el costo base. Si se informa <c>StockInicial</c> &gt; 0, el stock entra
    /// como un ajuste de inventario (kardex + asiento), por lo que exige el permiso de ajustes.
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
        var nombre = await ValidarDatosComercialesAsync(dto, null);
        if (dto.StockInicial > 0 && !(await _autorizacion.AuthorizeAsync(User, PermisosConst.InventarioAjustesCrear)).Succeeded)
            return Forbid();

        var producto = new Producto
        {
            Sku = dto.Sku.Trim(),
            Nombre = nombre,
            Descripcion = dto.Descripcion,
            CategoriaId = dto.CategoriaId,
            UnidadMedida = string.IsNullOrWhiteSpace(dto.UnidadMedida) ? "UN" : dto.UnidadMedida.Trim().ToUpperInvariant(),
            PrecioVenta = dto.PrecioVenta,
            CostoBase = dto.CostoBase,
            CostoPromedioPonderado = dto.CostoBase,
            StockMinimo = dto.StockMinimo,
            PuntoReorden = dto.PuntoReorden,
            CreadoPor = User.ObtenerUsuario()
        };
        await _context.Productos.AddAsync(producto);
        await _context.SaveChangesAsync();

        if (dto.StockInicial > 0)
            await _inventarioService.RegistrarAjusteAsync(
                new RegistrarAjusteInventarioDto(producto.Id, dto.StockInicial, "Stock inicial"), User.ObtenerUsuario());

        return Ok(await _context.Productos.AsNoTracking().FirstAsync(p => p.Id == producto.Id));
    }

    /// <summary>
    /// Reglas comunes de alta y edición: nombre único (sin distinguir mayúsculas), cantidades enteras según la unidad
    /// y costo base (neto) menor que el precio de venta neto (el precio de venta incluye IVA). Devuelve el nombre normalizado.
    /// </summary>
    private async Task<string> ValidarDatosComercialesAsync(CrearProductoDto dto, int? idActual)
    {
        var nombre = UnidadesMedidaConst.NormalizarNombre(dto.Nombre);
        if (nombre.Length == 0)
            throw new InvalidOperationException("El nombre del producto es obligatorio.");
        if (await _context.Productos.AnyAsync(p => p.Activo && p.Id != idActual && p.Nombre.ToUpper() == nombre))
            throw new InvalidOperationException($"Ya existe un producto con el nombre {nombre}.");

        var unidad = string.IsNullOrWhiteSpace(dto.UnidadMedida) ? "UN" : dto.UnidadMedida.Trim();
        UnidadesMedidaConst.ValidarCantidad(unidad, dto.StockMinimo, "El stock mínimo");
        UnidadesMedidaConst.ValidarCantidad(unidad, dto.PuntoReorden, "El punto de reorden");
        UnidadesMedidaConst.ValidarCantidad(unidad, dto.StockInicial, "El stock actual");

        var netoVenta = ImpuestosConst.NetoDesdeBruto(dto.PrecioVenta);
        if (dto.CostoBase >= netoVenta)
            throw new InvalidOperationException("El costo base debe ser menor que el precio de venta (neto, sin IVA).");
        return nombre;
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
        var nombre = UnidadesMedidaConst.NormalizarNombre(dto.Nombre);
        if (nombre.Length == 0)
            throw new InvalidOperationException("El nombre de la categoría es obligatorio.");
        if (await _context.Categorias.AnyAsync(c => c.Activo && c.Nombre.ToUpper() == nombre))
            throw new InvalidOperationException($"Ya existe una categoría con el nombre {nombre}.");

        // El código lo asigna el sistema: CAT-001, CAT-002... (siguiente al mayor existente).
        var codigos = await _context.Categorias.AsNoTracking().Select(c => c.Codigo).ToListAsync();
        var siguiente = codigos
            .Select(c => System.Text.RegularExpressions.Regex.Match(c ?? "", @"^CAT-(\d+)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            .Where(m => m.Success).Select(m => int.Parse(m.Groups[1].Value)).DefaultIfEmpty(0).Max() + 1;
        var codigo = $"CAT-{siguiente:000}";

        var categoria = new Categoria
        {
            Codigo = codigo,
            Nombre = nombre,
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
        var nombre = await ValidarDatosComercialesAsync(dto, id);
        UnidadesMedidaConst.ValidarCantidad(dto.UnidadMedida, producto.StockActual,
            $"El stock actual ({producto.StockActual:0.####}) del producto");

        producto.Sku = dto.Sku.Trim();
        producto.Nombre = nombre;
        producto.Descripcion = dto.Descripcion;
        producto.CategoriaId = dto.CategoriaId;
        producto.UnidadMedida = string.IsNullOrWhiteSpace(dto.UnidadMedida) ? "UN" : dto.UnidadMedida.Trim().ToUpperInvariant();
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
