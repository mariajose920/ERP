using ERP.Application.DTOs;
using ERP.Application.Interfaces;
using ERP.Domain.Entities.Contabilidad;
using ERP.Domain.Entities.Inventario;
using ERP.Domain.Entities.Ventas;
using ERP.Domain.Enums;
using ERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Services;

public class VentasService : IVentasService
{
    private readonly ErpDbContext _context;

    public VentasService(ErpDbContext context)
    {
        _context = context;
    }

    public async Task<Venta> EmitirVentaAsync(CrearVentaDto dto, string usuario)
    {
        using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            var cliente = await _context.Clientes.FindAsync(dto.ClienteId)
                ?? throw new InvalidOperationException("Cliente no encontrado.");

            decimal subtotal = 0;
            decimal totalDescuento = 0;
            decimal costoVentaTotal = 0;
            var detalles = new List<DetalleVenta>();

            // 1. Validate stock for all products
            foreach (var item in dto.Items)
            {
                var producto = await _context.Productos.FindAsync(item.ProductoId)
                    ?? throw new InvalidOperationException($"Producto {item.ProductoId} no encontrado.");

                if (producto.StockActual < item.Cantidad)
                {
                    throw new InvalidOperationException($"Stock insuficiente para '{producto.Nombre}'. Disponible: {producto.StockActual}, Solicitado: {item.Cantidad}");
                }

                var itemSubtotal = (item.Cantidad * item.PrecioUnitario) - item.Descuento;
                subtotal += itemSubtotal;
                totalDescuento += item.Descuento;

                var costoItem = item.Cantidad * producto.CostoPromedioPonderado;
                costoVentaTotal += costoItem;

                detalles.Add(new DetalleVenta
                {
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.PrecioUnitario,
                    CostoUnitarioSnapshot = producto.CostoPromedioPonderado,
                    Descuento = item.Descuento,
                    Subtotal = itemSubtotal,
                    CreadoPor = usuario
                });

                // Discount stock and add to Kardex
                var stockAnterior = producto.StockActual;
                var nuevoStock = stockAnterior - item.Cantidad;
                producto.StockActual = nuevoStock;

                var kardex = new MovimientoKardex
                {
                    ProductoId = producto.Id,
                    Fecha = DateTime.UtcNow,
                    TipoMovimiento = TipoMovimientoKardex.SalidaVenta,
                    DocumentoTipo = "FacturaVenta",
                    DocumentoNumero = "PENDIENTE",
                    CantidadEntrada = 0,
                    CostoUnitarioEntrada = 0,
                    CantidadSalida = item.Cantidad,
                    CostoUnitarioSalida = producto.CostoPromedioPonderado,
                    SaldoCantidad = nuevoStock,
                    CostoPromedioPonderadoResultante = producto.CostoPromedioPonderado,
                    SaldoValorizado = nuevoStock * producto.CostoPromedioPonderado,
                    Glosa = $"Venta a {cliente.RazonSocial}",
                    CreadoPor = usuario
                };

                await _context.MovimientosKardex.AddAsync(kardex);
            }

            var iva = Math.Round(subtotal * 0.19m, 2);
            var total = subtotal + iva;

            var countVenta = await _context.Ventas.CountAsync() + 1;
            var numeroFactura = $"FV-{DateTime.UtcNow.Year}-{countVenta:D5}";

            var venta = new Venta
            {
                NumeroFactura = numeroFactura,
                FechaEmision = DateTime.UtcNow,
                ClienteId = cliente.Id,
                Estado = dto.MetodoPago.Equals("Efectivo", StringComparison.OrdinalIgnoreCase) ? EstadoVenta.Pagada : EstadoVenta.Emitida,
                Subtotal = subtotal,
                ImpuestoIva = iva,
                Descuento = totalDescuento,
                Total = total,
                TotalPagado = dto.MetodoPago.Equals("Efectivo", StringComparison.OrdinalIgnoreCase) ? total : 0,
                SaldoPendiente = dto.MetodoPago.Equals("Efectivo", StringComparison.OrdinalIgnoreCase) ? 0 : total,
                CostoVentaTotal = costoVentaTotal,
                MetodoPago = dto.MetodoPago,
                Observaciones = dto.Observaciones,
                CreadoPor = usuario,
                Detalles = detalles
            };

            await _context.Ventas.AddAsync(venta);
            await _context.SaveChangesAsync();

            // Update Kardex document number with actual invoice number
            var kardexList = await _context.MovimientosKardex
                .Where(k => k.DocumentoNumero == "PENDIENTE" && k.CreadoPor == usuario)
                .ToListAsync();

            foreach (var k in kardexList)
            {
                k.DocumentoNumero = numeroFactura;
                k.DocumentoId = venta.Id;
            }

            // 2. Generate Automatic Accounting Entries:
            // (a) Asiento de Venta
            // Debe: Clientes (2.1.01.01) o Caja (1.1.01.01) [Total]
            // Haber: Ingresos por Ventas (4.1.01.01) [Subtotal]
            // Haber: IVA Débito Fiscal (2.1.02.01) [IVA]
            var ctaCobro = dto.MetodoPago.Equals("Efectivo", StringComparison.OrdinalIgnoreCase)
                ? await _context.CuentasContables.FirstOrDefaultAsync(c => c.Codigo == "1.1.01.01")
                : await _context.CuentasContables.FirstOrDefaultAsync(c => c.Codigo == "1.1.02.01");

            var ctaIngresoVenta = await _context.CuentasContables.FirstOrDefaultAsync(c => c.Codigo == "4.1.01.01");
            var ctaIvaDebito = await _context.CuentasContables.FirstOrDefaultAsync(c => c.Codigo == "2.1.02.01");

            if (ctaCobro != null && ctaIngresoVenta != null && ctaIvaDebito != null)
            {
                var countAsiento = await _context.AsientosContables.CountAsync() + 1;
                var asientoVenta = new AsientoContable
                {
                    NumeroAsiento = $"AS-{DateTime.UtcNow.Year}-{countAsiento:D5}",
                    Fecha = DateTime.UtcNow,
                    Glosa = $"Venta Factura N° {venta.NumeroFactura} - {cliente.RazonSocial}",
                    ModuloOrigen = "Ventas",
                    DocumentoOrigenTipo = "FacturaVenta",
                    DocumentoOrigenId = venta.Id,
                    TotalDebe = total,
                    TotalHaber = total,
                    CreadoPor = usuario,
                    Detalles = new List<DetalleAsientoContable>
                    {
                        new() { CuentaContableId = ctaCobro.Id, Debe = total, Haber = 0, GlosaLinea = "Cobro Venta / CxC", CreadoPor = usuario },
                        new() { CuentaContableId = ctaIngresoVenta.Id, Debe = 0, Haber = subtotal, GlosaLinea = "Ingreso por Ventas", CreadoPor = usuario },
                        new() { CuentaContableId = ctaIvaDebito.Id, Debe = 0, Haber = iva, GlosaLinea = "IVA Débito Fiscal", CreadoPor = usuario }
                    }
                };

                ctaCobro.SaldoActual += total;
                ctaIngresoVenta.SaldoActual += subtotal;
                ctaIvaDebito.SaldoActual += iva;

                await _context.AsientosContables.AddAsync(asientoVenta);
                await _context.SaveChangesAsync();

                venta.AsientoVentaId = asientoVenta.Id;
            }

            // (b) Asiento de Costo de Ventas
            // Debe: Costo de Ventas (5.1.01.01) [costoVentaTotal]
            // Haber: Inventario de Mercaderías (1.1.03.01) [costoVentaTotal]
            var ctaCostoVentas = await _context.CuentasContables.FirstOrDefaultAsync(c => c.Codigo == "5.1.01.01");
            var ctaInventario = await _context.CuentasContables.FirstOrDefaultAsync(c => c.Codigo == "1.1.03.01");

            if (ctaCostoVentas != null && ctaInventario != null && costoVentaTotal > 0)
            {
                var countAsiento2 = await _context.AsientosContables.CountAsync() + 1;
                var asientoCosto = new AsientoContable
                {
                    NumeroAsiento = $"AS-{DateTime.UtcNow.Year}-{countAsiento2:D5}",
                    Fecha = DateTime.UtcNow,
                    Glosa = $"Costo de Venta Factura N° {venta.NumeroFactura}",
                    ModuloOrigen = "Ventas",
                    DocumentoOrigenTipo = "CostoVenta",
                    DocumentoOrigenId = venta.Id,
                    TotalDebe = costoVentaTotal,
                    TotalHaber = costoVentaTotal,
                    CreadoPor = usuario,
                    Detalles = new List<DetalleAsientoContable>
                    {
                        new() { CuentaContableId = ctaCostoVentas.Id, Debe = costoVentaTotal, Haber = 0, GlosaLinea = "Costo de Ventas Reconocido", CreadoPor = usuario },
                        new() { CuentaContableId = ctaInventario.Id, Debe = 0, Haber = costoVentaTotal, GlosaLinea = "Salida de Mercadería por Venta", CreadoPor = usuario }
                    }
                };

                ctaCostoVentas.SaldoActual += costoVentaTotal;
                ctaInventario.SaldoActual -= costoVentaTotal;

                await _context.AsientosContables.AddAsync(asientoCosto);
                await _context.SaveChangesAsync();

                venta.AsientoCostoVentaId = asientoCosto.Id;
            }

            if (venta.SaldoPendiente > 0)
            {
                cliente.SaldoPendiente += venta.SaldoPendiente;
            }

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return venta;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<Venta> AnularVentaAsync(int ventaId, string motivo, string usuario)
    {
        using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            var venta = await _context.Ventas
                .Include(v => v.Detalles)
                .Include(v => v.Cliente)
                .FirstOrDefaultAsync(v => v.Id == ventaId)
                ?? throw new InvalidOperationException("Venta no encontrada.");

            if (venta.Estado == EstadoVenta.Anulada)
            {
                throw new InvalidOperationException("La venta ya está anulada.");
            }

            // Restore product stock and create Kardex return entry
            foreach (var detalle in venta.Detalles)
            {
                var producto = await _context.Productos.FindAsync(detalle.ProductoId);
                if (producto != null)
                {
                    var stockAnterior = producto.StockActual;
                    var nuevoStock = stockAnterior + detalle.Cantidad;
                    producto.StockActual = nuevoStock;

                    var kardex = new MovimientoKardex
                    {
                        ProductoId = producto.Id,
                        Fecha = DateTime.UtcNow,
                        TipoMovimiento = TipoMovimientoKardex.Devolucion,
                        DocumentoTipo = "AnulacionVenta",
                        DocumentoNumero = venta.NumeroFactura,
                        DocumentoId = venta.Id,
                        CantidadEntrada = detalle.Cantidad,
                        CostoUnitarioEntrada = detalle.CostoUnitarioSnapshot,
                        CantidadSalida = 0,
                        CostoUnitarioSalida = 0,
                        SaldoCantidad = nuevoStock,
                        CostoPromedioPonderadoResultante = producto.CostoPromedioPonderado,
                        SaldoValorizado = nuevoStock * producto.CostoPromedioPonderado,
                        Glosa = $"Anulación Venta: {motivo}",
                        CreadoPor = usuario
                    };

                    await _context.MovimientosKardex.AddAsync(kardex);
                }
            }

            if (venta.Cliente != null && venta.SaldoPendiente > 0)
            {
                venta.Cliente.SaldoPendiente = Math.Max(0, venta.Cliente.SaldoPendiente - venta.SaldoPendiente);
            }

            venta.Estado = EstadoVenta.Anulada;
            venta.Observaciones = $"{venta.Observaciones} | [ANULADA por {usuario}: {motivo}]";
            venta.ModificadoPor = usuario;
            venta.FechaModificacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return venta;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<PagoVenta> RegistrarPagoAsync(RegistrarPagoVentaDto dto, string usuario)
    {
        using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            var venta = await _context.Ventas
                .Include(v => v.Cliente)
                .FirstOrDefaultAsync(v => v.Id == dto.VentaId)
                ?? throw new InvalidOperationException("Venta no encontrada.");

            if (dto.Monto <= 0 || dto.Monto > venta.SaldoPendiente)
            {
                throw new InvalidOperationException($"Monto de pago inválido. Saldo pendiente actual: {venta.SaldoPendiente}");
            }

            var pago = new PagoVenta
            {
                VentaId = venta.Id,
                FechaPago = DateTime.UtcNow,
                Monto = dto.Monto,
                MetodoPago = dto.MetodoPago,
                ReferenciaComprobante = dto.ReferenciaComprobante,
                CreadoPor = usuario
            };

            venta.TotalPagado += dto.Monto;
            venta.SaldoPendiente -= dto.Monto;

            if (venta.SaldoPendiente == 0)
            {
                venta.Estado = EstadoVenta.Pagada;
            }
            else
            {
                venta.Estado = EstadoVenta.ParcialmentePagada;
            }

            if (venta.Cliente != null)
            {
                venta.Cliente.SaldoPendiente = Math.Max(0, venta.Cliente.SaldoPendiente - dto.Monto);
            }

            // Asiento contable de Pago:
            // Debe: Banco (1.1.01.02) o Caja (1.1.01.01) [Monto]
            // Haber: Clientes Nacionales (1.1.02.01) [Monto]
            var ctaDestino = dto.MetodoPago.Equals("Efectivo", StringComparison.OrdinalIgnoreCase)
                ? await _context.CuentasContables.FirstOrDefaultAsync(c => c.Codigo == "1.1.01.01")
                : await _context.CuentasContables.FirstOrDefaultAsync(c => c.Codigo == "1.1.01.02");

            var ctaClientes = await _context.CuentasContables.FirstOrDefaultAsync(c => c.Codigo == "1.1.02.01");

            if (ctaDestino != null && ctaClientes != null)
            {
                var countAsiento = await _context.AsientosContables.CountAsync() + 1;
                var asientoPago = new AsientoContable
                {
                    NumeroAsiento = $"AS-{DateTime.UtcNow.Year}-{countAsiento:D5}",
                    Fecha = DateTime.UtcNow,
                    Glosa = $"Pago Factura N° {venta.NumeroFactura} - Ref: {dto.ReferenciaComprobante}",
                    ModuloOrigen = "Ventas",
                    DocumentoOrigenTipo = "Pago",
                    DocumentoOrigenId = venta.Id,
                    TotalDebe = dto.Monto,
                    TotalHaber = dto.Monto,
                    CreadoPor = usuario,
                    Detalles = new List<DetalleAsientoContable>
                    {
                        new() { CuentaContableId = ctaDestino.Id, Debe = dto.Monto, Haber = 0, GlosaLinea = "Ingreso Fondos por Pago", CreadoPor = usuario },
                        new() { CuentaContableId = ctaClientes.Id, Debe = 0, Haber = dto.Monto, GlosaLinea = "Abono a Cuenta por Cobrar", CreadoPor = usuario }
                    }
                };

                ctaDestino.SaldoActual += dto.Monto;
                ctaClientes.SaldoActual -= dto.Monto;

                await _context.AsientosContables.AddAsync(asientoPago);
                await _context.SaveChangesAsync();

                pago.AsientoPagoId = asientoPago.Id;
            }

            await _context.PagosVenta.AddAsync(pago);
            await _context.SaveChangesAsync();

            await tx.CommitAsync();
            return pago;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<List<Venta>> GetVentasAsync()
    {
        return await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .Include(v => v.Pagos)
            .OrderByDescending(v => v.FechaEmision)
            .ToListAsync();
    }
}
