namespace ERP.Application.DTOs;

// Auth DTOs
public record LoginRequestDto(string Email, string Password);
public record LoginResponseDto(string Token, string NombreCompleto, string Email, string Rol, List<string> Permisos);
public record RegisterUserDto(string NombreCompleto, string Email, string Password, string Rut, int RolId);

// Compras DTOs
public record CrearOrdenCompraDto(
    int ProveedorId,
    DateTime? FechaEntregaEsperada,
    string? Observaciones,
    List<DetalleOrdenCompraItemDto> Items
);
public record DetalleOrdenCompraItemDto(int ProductoId, decimal Cantidad, decimal PrecioUnitario);

public record RegistrarRecepcionDto(
    int OrdenCompraId,
    string? GuiaDespachoProveedor,
    string? RecibidoPor,
    string? Observaciones,
    List<DetalleRecepcionItemDto> Items
);
public record DetalleRecepcionItemDto(int ProductoId, decimal CantidadRecibida);

public record AprobarFacturaCompraDto(
    int ProveedorId,
    int? OrdenCompraId,
    int? RecepcionCompraId,
    string NumeroFactura,
    DateTime FechaEmision,
    DateTime FechaVencimiento,
    List<DetalleFacturaCompraItemDto> Items
);
public record DetalleFacturaCompraItemDto(int ProductoId, decimal Cantidad, decimal PrecioUnitario);

// Ventas DTOs
public record CrearVentaDto(
    int ClienteId,
    string MetodoPago,
    string? Observaciones,
    List<DetalleVentaItemDto> Items
);
public record DetalleVentaItemDto(int ProductoId, decimal Cantidad, decimal PrecioUnitario, decimal Descuento);

public record RegistrarPagoVentaDto(
    int VentaId,
    decimal Monto,
    string MetodoPago,
    string? ReferenciaComprobante
);

// Inventario DTOs
public record RegistrarAjusteInventarioDto(
    int ProductoId,
    decimal CantidadAjuste,
    string Motivo,
    string UsuarioEmail
);

// Contabilidad DTOs
public record CrearAsientoManualDto(
    string Glosa,
    DateTime Fecha,
    List<DetalleAsientoManualItemDto> Lineas
);
public record DetalleAsientoManualItemDto(int CuentaContableId, decimal Debe, decimal Haber, string? GlosaLinea);

public record BalanceComprobacionFilaDto(
    string CodigoCuenta,
    string NombreCuenta,
    decimal TotalDebe,
    decimal TotalHaber,
    decimal SaldoDeudor,
    decimal SaldoAcreedor
);

public record EstadoResultadosDto(
    decimal IngresosPorVentas,
    decimal CostoDeVentas,
    decimal MargenBruto,
    decimal GastosOperacionales,
    decimal UtilidadNeta
);
