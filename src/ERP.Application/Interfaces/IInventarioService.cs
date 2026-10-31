// Módulo: Inventario — Responsable: Adán
using ERP.Application.DTOs;
using ERP.Domain.Entities.Inventario;
using ERP.Domain.Entities.Maestros;
using ERP.Domain.Enums;

namespace ERP.Application.Interfaces;

/// <summary>Stock, kardex, ajustes y alertas de reorden.</summary>
public interface IInventarioService
{
    /// <summary>Productos activos con su stock y CPP.</summary>
    Task<List<Producto>> GetStockActualAsync();

    /// <summary>Kardex por producto o general, filtrado por fechas, tipo de movimiento y paginado.</summary>
    Task<PaginaDto<MovimientoKardex>> GetKardexAsync(int? productoId, DateTime? desde, DateTime? hasta, TipoMovimientoKardex? tipoMovimiento, int pagina, int tamanoPagina);

    /// <summary>Kardex de un producto, filtrado por fechas y paginado (más reciente primero).</summary>
    Task<PaginaDto<MovimientoKardex>> GetKardexPorProductoAsync(int productoId, DateTime? desde, DateTime? hasta, int pagina, int tamanoPagina);

    /// <summary>
    /// Registra un ajuste de inventario, su movimiento de kardex y el asiento contable:
    /// salida → Debe Gastos/Mermas / Haber Inventario; entrada → Debe Inventario / Haber Gastos/Mermas.
    /// </summary>
    Task<AjusteInventario> RegistrarAjusteAsync(RegistrarAjusteInventarioDto dto, string usuario);

    /// <summary>Productos con stock menor o igual a su punto de reorden.</summary>
    Task<List<Producto>> GetAlertasReordenAsync();

    /// <summary>Valor total del inventario (Σ stock × CPP).</summary>
    Task<decimal> GetValorizacionTotalInventarioAsync();

    /// <summary>Últimos ajustes de inventario (más reciente primero).</summary>
    Task<List<AjusteInventario>> GetAjustesAsync(int top);
}
