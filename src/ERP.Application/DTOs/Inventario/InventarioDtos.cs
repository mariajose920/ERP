// Módulo: Inventario — Responsable: Adán
using System.ComponentModel.DataAnnotations;

namespace ERP.Application.DTOs;

/// <summary>
/// Ajuste manual de inventario. Positivo = ingreso, negativo = merma/pérdida.
/// Genera kardex y asiento contable. El usuario se toma del JWT (se eliminó UsuarioEmail, A6).
/// </summary>
public record RegistrarAjusteInventarioDto(
    [Range(1, int.MaxValue)] int ProductoId,
    decimal CantidadAjuste,
    [Required, StringLength(200)] string Motivo
);

/// <summary>Datos para crear un producto (sin stock ni CPP: esos campos los calcula el sistema).</summary>
public record CrearProductoDto(
    [Required, StringLength(50)] string Sku,
    [Required, StringLength(200)] string Nombre,
    [StringLength(500)] string? Descripcion,
    [Range(1, int.MaxValue)] int CategoriaId,
    [StringLength(10)] string? UnidadMedida,
    [Range(typeof(decimal), "0", "999999999999", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)] decimal PrecioVenta,
    [Range(typeof(decimal), "0", "999999999999", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)] decimal CostoBase,
    [Range(typeof(decimal), "0", "999999999999", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)] decimal StockMinimo,
    [Range(typeof(decimal), "0", "999999999999", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)] decimal PuntoReorden,
    [Range(typeof(decimal), "0", "999999999999", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)] decimal StockInicial = 0);

/// <summary>Datos para crear una categoría de productos.</summary>
public record CrearCategoriaDto(
    [Required, StringLength(20)] string Codigo,
    [Required, StringLength(100)] string Nombre,
    [StringLength(300)] string? Descripcion);

/// <summary>Página de resultados genérica.</summary>
public record PaginaDto<T>(IReadOnlyList<T> Items, int Pagina, int TamanoPagina, int TotalRegistros);
