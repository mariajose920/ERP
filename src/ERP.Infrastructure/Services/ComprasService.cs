using ERP.Application.DTOs;
using ERP.Application.Interfaces;
using ERP.Domain.Entities.Compras;
using ERP.Domain.Entities.Contabilidad;
using ERP.Domain.Entities.Inventario;
using ERP.Domain.Enums;
using ERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Services;

public class ComprasService : IComprasService
{
    private readonly ErpDbContext _context;

    public ComprasService(ErpDbContext context)
    {
        _context = context;
    }

    public async Task<OrdenCompra> CrearOrdenCompraAsync(CrearOrdenCompraDto dto, string usuario)
    {
        var count = await _context.OrdenesCompra.CountAsync() + 1;
        var numeroOrden = $"OC-{DateTime.UtcNow.Year}-{count:D5}";

        decimal subtotal = 0;
        var detalles = new List<DetalleOrdenCompra>();

        foreach (var item in dto.Items)
        {
            var itemSubtotal = item.Cantidad * item.PrecioUnitario;
            subtotal += itemSubtotal;
            detalles.Add(new DetalleOrdenCompra
            {
                ProductoId = item.ProductoId,
                Cantidad = item.Cantidad,
                CantidadRecibida = 0,
                PrecioUnitario = item.PrecioUnitario,
                Subtotal = itemSubtotal,
                CreadoPor = usuario
            });
        }

        var iva = Math.Round(subtotal * 0.19m, 2);
        var total = subtotal + iva;

        var orden = new OrdenCompra
        {
            NumeroOrden = numeroOrden,
            FechaEmision = DateTime.UtcNow,
            FechaEntregaEsperada = dto.FechaEntregaEsperada,
            ProveedorId = dto.ProveedorId,
            Estado = EstadoOrdenCompra.Pendiente,
            Subtotal = subtotal,
            ImpuestoIva = iva,
            Total = total,
            Observaciones = dto.Observaciones,
            CreadoPor = usuario,
            Detalles = detalles
        };

        await _context.OrdenesCompra.AddAsync(orden);
        await _context.SaveChangesAsync();

        return orden;
    }

    public async Task<RecepcionCompra> RegistrarRecepcionAsync(RegistrarRecepcionDto dto, string usuario)
    {
        using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            var orden = await _context.OrdenesCompra
                .Include(o => o.Detalles)
                .FirstOrDefaultAsync(o => o.Id == dto.OrdenCompraId)
                ?? throw new InvalidOperationException("Orden de compra no encontrada.");

            var count = await _context.RecepcionesCompra.CountAsync() + 1;
            var numeroRecepcion = $"RC-{DateTime.UtcNow.Year}-{count:D5}";

            var recepcion = new RecepcionCompra
            {
                NumeroRecepcion = numeroRecepcion,
                FechaRecepcion = DateTime.UtcNow,
                OrdenCompraId = orden.Id,
                GuiaDespachoProveedor = dto.GuiaDespachoProveedor,
                RecibidoPor = dto.RecibidoPor ?? usuario,
                Observaciones = dto.Observaciones,
                CreadoPor = usuario,
                Detalles = new List<DetalleRecepcionCompra>()
            };

            foreach (var item in dto.Items)
            {
                var detalleOrden = orden.Detalles.FirstOrDefault(d => d.ProductoId == item.ProductoId)
                    ?? throw new InvalidOperationException($"El producto {item.ProductoId} no pertenece a esta orden de compra.");

                if (detalleOrden.CantidadRecibida + item.CantidadRecibida > detalleOrden.Cantidad)
                {
                    throw new InvalidOperationException($"La cantidad recibida excede la cantidad pendiente en la orden.");
                }

                detalleOrden.CantidadRecibida += item.CantidadRecibida;

                recepcion.Detalles.Add(new DetalleRecepcionCompra
                {
                    ProductoId = item.ProductoId,
                    CantidadRecibida = item.CantidadRecibida,
                    CreadoPor = usuario
                });
            }

            // Update order status
            bool todasCompletas = orden.Detalles.All(d => d.CantidadRecibida >= d.Cantidad);
            orden.Estado = todasCompletas ? EstadoOrdenCompra.RecibidaTotal : EstadoOrdenCompra.RecibidaParcial;

            await _context.RecepcionesCompra.AddAsync(recepcion);
            await _context.SaveChangesAsync();

            await tx.CommitAsync();
            return recepcion;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<FacturaCompra> RegistrarYAprobarFacturaCompraAsync(AprobarFacturaCompraDto dto, string usuario)
    {
        using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            decimal subtotal = 0;
            var detalles = new List<DetalleFacturaCompra>();

            foreach (var item in dto.Items)
            {
                var itemSub = item.Cantidad * item.PrecioUnitario;
                subtotal += itemSub;
                detalles.Add(new DetalleFacturaCompra
                {
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.PrecioUnitario,
                    Subtotal = itemSub,
                    CreadoPor = usuario
                });
            }

            var iva = Math.Round(subtotal * 0.19m, 2);
            var total = subtotal + iva;

            var factura = new FacturaCompra
            {
                NumeroFactura = dto.NumeroFactura,
                FechaEmision = dto.FechaEmision.ToUniversalTime(),
                FechaVencimiento = dto.FechaVencimiento.ToUniversalTime(),
                ProveedorId = dto.ProveedorId,
                OrdenCompraId = dto.OrdenCompraId,
                RecepcionCompraId = dto.RecepcionCompraId,
                Estado = EstadoFacturaCompra.Aprobada,
                Subtotal = subtotal,
                ImpuestoIva = iva,
                Total = total,
                CreadoPor = usuario,
                Detalles = detalles
            };

            await _context.FacturasCompra.AddAsync(factura);
            await _context.SaveChangesAsync();

            // 1. Update stock, Kardex and CPP
            foreach (var item in dto.Items)
            {
                var producto = await _context.Productos.FindAsync(item.ProductoId)
                    ?? throw new InvalidOperationException($"Producto {item.ProductoId} no encontrado.");

                var stockAnterior = producto.StockActual;
                var cppAnterior = producto.CostoPromedioPonderado;
                var nuevoStock = stockAnterior + item.Cantidad;

                // Recalculate CPP: ((StockAnt * CPPAnt) + (CantComprada * CostoUnit)) / NuevoStock
                decimal nuevoCpp = nuevoStock > 0
                    ? ((stockAnterior * cppAnterior) + (item.Cantidad * item.PrecioUnitario)) / nuevoStock
                    : item.PrecioUnitario;

                producto.StockActual = nuevoStock;
                producto.CostoPromedioPonderado = nuevoCpp;
                producto.CostoBase = item.PrecioUnitario;

                // Create Kardex movement
                var kardex = new MovimientoKardex
                {
                    ProductoId = producto.Id,
                    Fecha = DateTime.UtcNow,
                    TipoMovimiento = TipoMovimientoKardex.EntradaCompra,
                    DocumentoTipo = "FacturaCompra",
                    DocumentoNumero = factura.NumeroFactura,
                    DocumentoId = factura.Id,
                    CantidadEntrada = item.Cantidad,
                    CostoUnitarioEntrada = item.PrecioUnitario,
                    CantidadSalida = 0,
                    CostoUnitarioSalida = 0,
                    SaldoCantidad = nuevoStock,
                    CostoPromedioPonderadoResultante = nuevoCpp,
                    SaldoValorizado = nuevoStock * nuevoCpp,
                    Glosa = $"Compra aprobada Factura {factura.NumeroFactura}",
                    CreadoPor = usuario
                };

                await _context.MovimientosKardex.AddAsync(kardex);
            }

            // 2. Generate Automatic Accounting Entry:
            // Debe: Inventario (1.1.03.01) [Subtotal]
            // Debe: IVA Crédito Fiscal (1.1.04.01) [IVA]
            // Haber: Proveedores (2.1.01.01) [Total]
            var ctaInventario = await _context.CuentasContables.FirstOrDefaultAsync(c => c.Codigo == "1.1.03.01");
            var ctaIvaCredito = await _context.CuentasContables.FirstOrDefaultAsync(c => c.Codigo == "1.1.04.01");
            var ctaProveedores = await _context.CuentasContables.FirstOrDefaultAsync(c => c.Codigo == "2.1.01.01");

            if (ctaInventario != null && ctaIvaCredito != null && ctaProveedores != null)
            {
                var countAsiento = await _context.AsientosContables.CountAsync() + 1;
                var asiento = new AsientoContable
                {
                    NumeroAsiento = $"AS-{DateTime.UtcNow.Year}-{countAsiento:D5}",
                    Fecha = DateTime.UtcNow,
                    Glosa = $"Centralización Factura Compra N° {factura.NumeroFactura}",
                    ModuloOrigen = "Compras",
                    DocumentoOrigenTipo = "FacturaCompra",
                    DocumentoOrigenId = factura.Id,
                    TotalDebe = total,
                    TotalHaber = total,
                    CreadoPor = usuario,
                    Detalles = new List<DetalleAsientoContable>
                    {
                        new() { CuentaContableId = ctaInventario.Id, Debe = subtotal, Haber = 0, GlosaLinea = "Ingreso Mercadería", CreadoPor = usuario },
                        new() { CuentaContableId = ctaIvaCredito.Id, Debe = iva, Haber = 0, GlosaLinea = "IVA Crédito Fiscal", CreadoPor = usuario },
                        new() { CuentaContableId = ctaProveedores.Id, Debe = 0, Haber = total, GlosaLinea = "Cuenta por Pagar Proveedor", CreadoPor = usuario }
                    }
                };

                ctaInventario.SaldoActual += subtotal;
                ctaIvaCredito.SaldoActual += iva;
                ctaProveedores.SaldoActual += total;

                await _context.AsientosContables.AddAsync(asiento);
                await _context.SaveChangesAsync();

                factura.AsientoContableId = asiento.Id;
            }

            if (factura.OrdenCompraId.HasValue)
            {
                var oc = await _context.OrdenesCompra.FindAsync(factura.OrdenCompraId.Value);
                if (oc != null) oc.Estado = EstadoOrdenCompra.Liquidada;
            }

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return factura;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<List<OrdenCompra>> GetOrdenesAsync()
    {
        return await _context.OrdenesCompra
            .Include(o => o.Proveedor)
            .Include(o => o.Detalles)
                .ThenInclude(d => d.Producto)
            .OrderByDescending(o => o.FechaEmision)
            .ToListAsync();
    }

    public async Task<List<FacturaCompra>> GetFacturasAsync()
    {
        return await _context.FacturasCompra
            .Include(f => f.Proveedor)
            .Include(f => f.Detalles)
                .ThenInclude(d => d.Producto)
            .OrderByDescending(f => f.FechaEmision)
            .ToListAsync();
    }
}
