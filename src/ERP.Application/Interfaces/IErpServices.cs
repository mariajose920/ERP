using ERP.Application.DTOs;
using ERP.Domain.Entities.Compras;
using ERP.Domain.Entities.Contabilidad;
using ERP.Domain.Entities.Inventario;
using ERP.Domain.Entities.Maestros;
using ERP.Domain.Entities.Seguridad;
using ERP.Domain.Entities.Ventas;

namespace ERP.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
    Task<Usuario> RegisterUserAsync(RegisterUserDto request, string creadorEmail);
}

public interface IComprasService
{
    Task<OrdenCompra> CrearOrdenCompraAsync(CrearOrdenCompraDto dto, string usuario);
    Task<RecepcionCompra> RegistrarRecepcionAsync(RegistrarRecepcionDto dto, string usuario);
    Task<FacturaCompra> RegistrarYAprobarFacturaCompraAsync(AprobarFacturaCompraDto dto, string usuario);
    Task<List<OrdenCompra>> GetOrdenesAsync();
    Task<List<FacturaCompra>> GetFacturasAsync();
}

public interface IVentasService
{
    Task<Venta> EmitirVentaAsync(CrearVentaDto dto, string usuario);
    Task<Venta> AnularVentaAsync(int ventaId, string motivo, string usuario);
    Task<PagoVenta> RegistrarPagoAsync(RegistrarPagoVentaDto dto, string usuario);
    Task<List<Venta>> GetVentasAsync();
}

public interface IInventarioService
{
    Task<List<Producto>> GetStockActualAsync();
    Task<List<MovimientoKardex>> GetKardexPorProductoAsync(int productoId);
    Task<AjusteInventario> RegistrarAjusteAsync(RegistrarAjusteInventarioDto dto, string usuario);
    Task<List<Producto>> GetAlertasReordenAsync();
    Task<decimal> GetValorizacionTotalInventarioAsync();
}

public interface IContabilidadService
{
    Task<AsientoContable> CrearAsientoAsync(CrearAsientoManualDto dto, string usuario);
    Task<List<AsientoContable>> GetLibroDiarioAsync(DateTime? desde, DateTime? hasta);
    Task<List<DetalleAsientoContable>> GetLibroMayorPorCuentaAsync(int cuentaId, DateTime? desde, DateTime? hasta);
    Task<List<BalanceComprobacionFilaDto>> GetBalanceComprobacionAsync();
    Task<EstadoResultadosDto> GetEstadoResultadosAsync(DateTime? desde, DateTime? hasta);
}
