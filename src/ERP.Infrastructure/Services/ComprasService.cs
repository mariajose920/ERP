// Módulo: Compras — Responsable: Adán
using ERP.Application.Common;
using ERP.Application.DTOs;
using ERP.Application.Interfaces;
using ERP.Domain.Entities.Compras;
using ERP.Domain.Entities.Inventario;
using ERP.Domain.Enums;
using ERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Services;

/// <summary>
/// Ciclo de compras. La recepción ingresa el stock (CPP y kardex al precio de la OC) y genera el asiento
/// Inventario / Recepciones por facturar; la factura salda esa cuenta transitoria contra Proveedores (+ IVA).
/// </summary>
public class ComprasService : IComprasService
{
    private readonly ErpDbContext _context;
    private readonly INumeradorDocumentos _numerador;
    private readonly IContabilidadService _contabilidad;

    private const string DocRecepcion = "RecepcionCompra";
    private const string DocFactura = "FacturaCompra";

    public ComprasService(ErpDbContext context, INumeradorDocumentos numerador, IContabilidadService contabilidad)
    {
        _context = context;
        _numerador = numerador;
        _contabilidad = contabilidad;
    }

    /// <inheritdoc />
    public Task<OrdenCompra> CrearOrdenCompraAsync(CrearOrdenCompraDto dto, string usuario)
    {
        return _context.EnTransaccionAsync(async () =>
        {
            if (dto.FechaEntregaEsperada.HasValue && dto.FechaEntregaEsperada.Value.Date < DateTime.UtcNow.Date)
            {
                throw new InvalidOperationException("La fecha esperada de entrega no puede ser anterior a la fecha actual.");
            }

            await ValidarProveedorActivoAsync(dto.ProveedorId);
            await ValidarProductosAsync(dto.Items.Select(i => i.ProductoId));

            var detalles = dto.Items.Select(item => new DetalleOrdenCompra
            {
                ProductoId = item.ProductoId,
                Cantidad = item.Cantidad,
                CantidadRecibida = 0,
                PrecioUnitario = item.PrecioUnitario,
                Subtotal = item.Cantidad * item.PrecioUnitario,
                CreadoPor = usuario
            }).ToList();

            var subtotal = detalles.Sum(d => d.Subtotal);
            var iva = ImpuestosConst.CalcularIva(subtotal);

            var orden = new OrdenCompra
            {
                NumeroOrden = await _numerador.SiguienteAsync(TipoDocumentoNumerado.OrdenCompra),
                FechaEmision = DateTime.UtcNow,
                FechaEntregaEsperada = dto.FechaEntregaEsperada,
                ProveedorId = dto.ProveedorId,
                Estado = EstadoOrdenCompra.Pendiente,
                Subtotal = subtotal,
                ImpuestoIva = iva,
                Total = subtotal + iva,
                Observaciones = dto.Observaciones,
                CreadoPor = usuario,
                Detalles = detalles
            };

            await _context.OrdenesCompra.AddAsync(orden);
            await _context.SaveChangesAsync();
            return orden;
        });
    }

    /// <inheritdoc />
    public Task<RecepcionCompra> RegistrarRecepcionAsync(RegistrarRecepcionDto dto, string usuario)
    {
        return _context.EnTransaccionAsync(async () =>
        {
            var orden = await _context.OrdenesCompra
                .Include(o => o.Detalles)
                .FirstOrDefaultAsync(o => o.Id == dto.OrdenCompraId)
                ?? throw new KeyNotFoundException("Orden de compra no encontrada.");

            if (orden.Estado == EstadoOrdenCompra.Pendiente)
                throw new InvalidOperationException($"La orden {orden.NumeroOrden} aún no está aprobada; apruébela antes de recibir mercadería.");
            if (orden.Estado is EstadoOrdenCompra.Anulada or EstadoOrdenCompra.Liquidada or EstadoOrdenCompra.RecibidaTotal)
                throw new InvalidOperationException($"La orden {orden.NumeroOrden} está en estado {orden.Estado} y no admite recepciones.");

            var recepcion = new RecepcionCompra
            {
                NumeroRecepcion = await _numerador.SiguienteAsync(TipoDocumentoNumerado.RecepcionCompra),
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
                    throw new InvalidOperationException(
                        $"La cantidad recibida del producto {item.ProductoId} excede lo pendiente ({detalleOrden.Cantidad - detalleOrden.CantidadRecibida}).");

                detalleOrden.CantidadRecibida += item.CantidadRecibida;

                recepcion.Detalles.Add(new DetalleRecepcionCompra
                {
                    ProductoId = item.ProductoId,
                    DetalleOrdenCompraId = detalleOrden.Id,
                    CantidadRecibida = item.CantidadRecibida,
                    CreadoPor = usuario
                });
            }

            // Estado de la orden según lo recibido.
            var todasCompletas = orden.Detalles.All(d => d.CantidadRecibida >= d.Cantidad);
            orden.Estado = todasCompletas ? EstadoOrdenCompra.RecibidaTotal : EstadoOrdenCompra.RecibidaParcial;

            await _context.RecepcionesCompra.AddAsync(recepcion);
            await _context.SaveChangesAsync();

            // La recepción ingresa el stock de inmediato (sin esperar la factura), valorizado al precio de la OC:
            // stock, CPP y kardex por producto, y el asiento Debe Inventario / Haber Recepciones por facturar.
            decimal netoRecibido = 0;
            foreach (var item in dto.Items)
            {
                var precio = orden.Detalles.First(d => d.ProductoId == item.ProductoId).PrecioUnitario;
                var producto = await _context.Productos.FindAsync(item.ProductoId)
                    ?? throw new KeyNotFoundException($"Producto {item.ProductoId} no encontrado.");

                var stockAnterior = producto.StockActual;
                var nuevoStock = stockAnterior + item.CantidadRecibida;
                var nuevoCpp = InventarioService.CalcularCpp(stockAnterior, producto.CostoPromedioPonderado, item.CantidadRecibida, precio);
                producto.StockActual = nuevoStock;
                producto.CostoPromedioPonderado = nuevoCpp;
                producto.CostoBase = precio;
                netoRecibido += item.CantidadRecibida * precio;

                await _context.MovimientosKardex.AddAsync(new MovimientoKardex
                {
                    ProductoId = producto.Id,
                    Fecha = DateTime.UtcNow,
                    TipoMovimiento = TipoMovimientoKardex.EntradaCompra,
                    DocumentoTipo = DocRecepcion,
                    DocumentoNumero = recepcion.NumeroRecepcion,
                    DocumentoId = recepcion.Id,
                    CantidadEntrada = item.CantidadRecibida,
                    CostoUnitarioEntrada = precio,
                    SaldoCantidad = nuevoStock,
                    CostoPromedioPonderadoResultante = nuevoCpp,
                    SaldoValorizado = nuevoStock * nuevoCpp,
                    Glosa = $"Recepción {recepcion.NumeroRecepcion} (OC {orden.NumeroOrden})",
                    CreadoPor = usuario
                });
            }

            if (netoRecibido > 0)
            {
                var (codDebe, codHaber) = await _contabilidad.ResolverCuentasAsync("Compras", MatrizIntegracionSemilla.RecepcionMercaderia,
                    CuentasContablesConst.Inventario, CuentasContablesConst.RecepcionesPorFacturar);
                await _contabilidad.RegistrarAsientoAutomaticoAsync(new AsientoAutomaticoDto(
                    $"Recepción de mercadería {recepcion.NumeroRecepcion} - OC {orden.NumeroOrden}",
                    "Compras", DocRecepcion, recepcion.Id,
                    new List<LineaAsientoAutomaticoDto>
                    {
                        new(codDebe, netoRecibido, 0, "Ingreso de mercadería recibida"),
                        new(codHaber, 0, netoRecibido, "Recepción pendiente de factura")
                    }), usuario);
            }

            await _context.SaveChangesAsync();
            return recepcion;
        });
    }

    /// <inheritdoc />
    public Task<FacturaCompra> RegistrarYAprobarFacturaCompraAsync(AprobarFacturaCompraDto dto, string usuario)
    {
        return _context.EnTransaccionAsync(async () =>
        {
            // A3: validaciones de existencia y coherencia con la OC/recepción.

            await ValidarProveedorActivoAsync(dto.ProveedorId);

            if (await _context.FacturasCompra.AnyAsync(f => f.ProveedorId == dto.ProveedorId && f.NumeroFactura == dto.NumeroFactura && f.Estado != EstadoFacturaCompra.Anulada))
                throw new InvalidOperationException($"La factura {dto.NumeroFactura} de este proveedor ya fue registrada.");

            if (dto.FechaVencimiento < dto.FechaEmision)
                throw new InvalidOperationException("La fecha de vencimiento no puede ser anterior a la de emisión.");

            OrdenCompra? orden = null;
            if (dto.OrdenCompraId.HasValue)
            {
                orden = await _context.OrdenesCompra.Include(o => o.Detalles)
                    .FirstOrDefaultAsync(o => o.Id == dto.OrdenCompraId.Value)
                    ?? throw new KeyNotFoundException("Orden de compra no encontrada.");

                if (orden.ProveedorId != dto.ProveedorId)
                    throw new InvalidOperationException("La orden de compra pertenece a otro proveedor.");
                if (orden.Estado is EstadoOrdenCompra.Anulada or EstadoOrdenCompra.Liquidada or EstadoOrdenCompra.Pendiente or EstadoOrdenCompra.Aprobada)
                    throw new InvalidOperationException($"La orden {orden.NumeroOrden} está {orden.Estado} y no admite facturas (requiere mercadería recibida).");

                // Lo ya facturado (aprobado) de esta OC, por producto.
                var yaFacturado = await _context.DetallesFacturaCompra
                    .Where(d => d.FacturaCompra!.OrdenCompraId == orden.Id && d.FacturaCompra.Estado == EstadoFacturaCompra.Aprobada)
                    .GroupBy(d => d.ProductoId)
                    .Select(g => new { ProductoId = g.Key, Cantidad = g.Sum(x => x.Cantidad) })
                    .ToDictionaryAsync(x => x.ProductoId, x => x.Cantidad);

                foreach (var grupo in dto.Items.GroupBy(i => i.ProductoId))
                {
                    var detalle = orden.Detalles.FirstOrDefault(d => d.ProductoId == grupo.Key)
                        ?? throw new InvalidOperationException($"El producto {grupo.Key} no pertenece a la orden {orden.NumeroOrden}.");
                    var facturado = yaFacturado.GetValueOrDefault(grupo.Key) + grupo.Sum(i => i.Cantidad);
                    if (facturado > detalle.CantidadRecibida)
                        throw new InvalidOperationException(
                            $"Producto {grupo.Key}: lo facturado ({facturado}) supera lo recibido ({detalle.CantidadRecibida}). Registre primero la recepción.");
                }
            }

            if (dto.RecepcionCompraId.HasValue)
            {
                var recepcion = await _context.RecepcionesCompra.FindAsync(dto.RecepcionCompraId.Value)
                    ?? throw new KeyNotFoundException("Recepción de compra no encontrada.");
                if (orden == null || recepcion.OrdenCompraId != orden.Id)
                    throw new InvalidOperationException("La recepción indicada no corresponde a la orden de compra de la factura.");
            }

            await ValidarProductosAsync(dto.Items.Select(i => i.ProductoId));

            var detalles = dto.Items.Select(item => new DetalleFacturaCompra
            {
                ProductoId = item.ProductoId,
                Cantidad = item.Cantidad,
                PrecioUnitario = item.PrecioUnitario,
                Subtotal = item.Cantidad * item.PrecioUnitario,
                CreadoPor = usuario
            }).ToList();

            var subtotal = detalles.Sum(d => d.Subtotal);
            var iva = ImpuestosConst.CalcularIva(subtotal);
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
                TotalPagado = 0,
                Subtotal = subtotal,
                ImpuestoIva = iva,
                Total = total,
                SaldoPendiente = total,
                CreadoPor = usuario,
                Detalles = detalles
            };

            // Cantidad por producto que ya entró al stock vía recepción y aún no se factura (se calcula antes de
            // guardar esta factura). Lo que exceda (recepciones antiguas, previas a este cambio, o facturas sin OC)
            // ingresa el stock aquí, como antes.
            var pendienteTransito = await PendienteDeFacturarPorRecepcionAsync(orden);

            await _context.FacturasCompra.AddAsync(factura);
            await _context.SaveChangesAsync();

            decimal netoTransito = 0, netoDirecto = 0, diferenciaPrecio = 0;
            foreach (var item in dto.Items)
            {
                var producto = await _context.Productos.FindAsync(item.ProductoId)
                    ?? throw new KeyNotFoundException($"Producto {item.ProductoId} no encontrado.");

                // 1a. Parte ya ingresada por la recepción: salda la cuenta transitoria al precio de la OC; la
                //     diferencia con el precio facturado ajusta el inventario y el CPP.
                var disponible = pendienteTransito.GetValueOrDefault(item.ProductoId);
                var cantTransito = Math.Min(item.Cantidad, Math.Max(disponible, 0));
                pendienteTransito[item.ProductoId] = disponible - cantTransito;
                if (cantTransito > 0)
                {
                    var precioOc = orden!.Detalles.First(d => d.ProductoId == item.ProductoId).PrecioUnitario;
                    netoTransito += cantTransito * precioOc;
                    var diferencia = cantTransito * (item.PrecioUnitario - precioOc);
                    diferenciaPrecio += diferencia;
                    if (diferencia != 0 && producto.StockActual > 0)
                        producto.CostoPromedioPonderado = Math.Max(0, producto.CostoPromedioPonderado + diferencia / producto.StockActual);
                    producto.CostoBase = item.PrecioUnitario;
                }

                var cantDirecta = item.Cantidad - cantTransito;
                if (cantDirecta <= 0) continue;
                netoDirecto += cantDirecta * item.PrecioUnitario;

                // 1b. Ingreso de stock, CPP y kardex (el CPP se calcula con InventarioService.CalcularCpp, A4).
                var stockAnterior = producto.StockActual;
                var nuevoStock = stockAnterior + cantDirecta;
                var nuevoCpp = InventarioService.CalcularCpp(stockAnterior, producto.CostoPromedioPonderado, cantDirecta, item.PrecioUnitario);

                producto.StockActual = nuevoStock;
                producto.CostoPromedioPonderado = nuevoCpp;
                producto.CostoBase = item.PrecioUnitario;

                await _context.MovimientosKardex.AddAsync(new MovimientoKardex
                {
                    ProductoId = producto.Id,
                    Fecha = DateTime.UtcNow,
                    TipoMovimiento = TipoMovimientoKardex.EntradaCompra,
                    DocumentoTipo = DocFactura,
                    DocumentoNumero = factura.NumeroFactura,
                    DocumentoId = factura.Id,
                    CantidadEntrada = cantDirecta,
                    CostoUnitarioEntrada = item.PrecioUnitario,
                    SaldoCantidad = nuevoStock,
                    CostoPromedioPonderadoResultante = nuevoCpp,
                    SaldoValorizado = nuevoStock * nuevoCpp,
                    Glosa = $"Compra aprobada Factura {factura.NumeroFactura}",
                    CreadoPor = usuario
                });
            }

            // 2. Asiento de compra (saldos vía ContabilidadService.AplicarMovimiento, A5/K3):
            //    Debe  Recepciones por facturar (2.1.01.02) [neto ya recibido, a precio de la OC]
            //    Debe/Haber Inventario (1.1.03.01)          [diferencia de precio factura vs OC]
            //    Debe  Inventario (1.1.03.01)               [neto que ingresa stock aquí: sin recepción previa]
            //    Debe  IVA Crédito Fiscal (1.1.04.01)       [IVA]
            //    Haber Proveedores (2.1.01.01)              [total]
            var lineas = new List<LineaAsientoAutomaticoDto>();
            if (netoTransito > 0) lineas.Add(new(CuentasContablesConst.RecepcionesPorFacturar, netoTransito, 0, "Salda mercadería recibida por facturar"));
            var inventarioDebe = netoDirecto + diferenciaPrecio;
            if (inventarioDebe > 0) lineas.Add(new(CuentasContablesConst.Inventario, inventarioDebe, 0, netoDirecto > 0 ? "Ingreso Mercadería" : "Ajuste por diferencia de precio"));
            if (inventarioDebe < 0) lineas.Add(new(CuentasContablesConst.Inventario, 0, -inventarioDebe, "Ajuste por diferencia de precio"));
            if (iva > 0) lineas.Add(new(CuentasContablesConst.IvaCreditoFiscal, iva, 0, "IVA Crédito Fiscal"));
            lineas.Add(new(CuentasContablesConst.Proveedores, 0, total, "Cuenta por Pagar Proveedor"));

            var asiento = await _contabilidad.RegistrarAsientoAutomaticoAsync(new AsientoAutomaticoDto(
                $"Centralización Factura Compra N° {factura.NumeroFactura}",
                "Compras", DocFactura, factura.Id, lineas), usuario);

            factura.AsientoContableId = asiento.Id;

            // 3. La OC queda Liquidada solo cuando todo lo pedido fue recibido y facturado.
            if (orden != null)
            {
                var facturadoTotal = await _context.DetallesFacturaCompra
                    .Where(d => d.FacturaCompra!.OrdenCompraId == orden.Id && d.FacturaCompra.Estado == EstadoFacturaCompra.Aprobada)
                    .GroupBy(d => d.ProductoId)
                    .Select(g => new { ProductoId = g.Key, Cantidad = g.Sum(x => x.Cantidad) })
                    .ToDictionaryAsync(x => x.ProductoId, x => x.Cantidad);

                if (orden.Detalles.All(d => facturadoTotal.GetValueOrDefault(d.ProductoId) >= d.Cantidad))
                    orden.Estado = EstadoOrdenCompra.Liquidada;
            }

            await _context.SaveChangesAsync();
            return factura;
        });
    }

    /// <inheritdoc />
    public async Task<List<OrdenCompra>> GetOrdenesAsync()
    {
        return await _context.OrdenesCompra
            .Include(o => o.Proveedor)
            .Include(o => o.Detalles)
                .ThenInclude(d => d.Producto)
            .AsNoTracking()
            .OrderByDescending(o => o.FechaEmision)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<FacturaCompra>> GetFacturasAsync()
    {
        return await _context.FacturasCompra
            .Include(f => f.Proveedor)
            .Include(f => f.Detalles)
                .ThenInclude(d => d.Producto)
            .Include(f => f.Pagos)
            .AsNoTracking()
            .OrderByDescending(f => f.FechaEmision)
            .ToListAsync();
    }

    /// <summary>
    /// Por producto de la OC: unidades que ingresaron al stock vía recepción y todavía no se facturan.
    /// = entradas de kardex de las recepciones de la OC - (facturado aprobado - lo que esas facturas ingresaron
    /// directamente al stock, que corresponde a facturas del flujo antiguo).
    /// </summary>
    private async Task<Dictionary<int, decimal>> PendienteDeFacturarPorRecepcionAsync(OrdenCompra? orden)
    {
        if (orden == null) return new Dictionary<int, decimal>();

        var idsRecepciones = await _context.RecepcionesCompra.Where(r => r.OrdenCompraId == orden.Id).Select(r => r.Id).ToListAsync();
        var idsFacturas = await _context.FacturasCompra
            .Where(f => f.OrdenCompraId == orden.Id && f.Estado == EstadoFacturaCompra.Aprobada).Select(f => f.Id).ToListAsync();

        var recibido = await SumarEntradasKardexAsync(DocRecepcion, idsRecepciones);
        var ingresadoPorFactura = await SumarEntradasKardexAsync(DocFactura, idsFacturas);
        var facturado = await _context.DetallesFacturaCompra
            .Where(d => idsFacturas.Contains(d.FacturaCompraId))
            .GroupBy(d => d.ProductoId).Select(g => new { g.Key, C = g.Sum(x => x.Cantidad) })
            .ToDictionaryAsync(x => x.Key, x => x.C);

        return recibido.ToDictionary(
            r => r.Key,
            r => r.Value - (facturado.GetValueOrDefault(r.Key) - ingresadoPorFactura.GetValueOrDefault(r.Key)));
    }

    /// <summary>Unidades que entraron al kardex por producto, para documentos de un tipo.</summary>
    private Task<Dictionary<int, decimal>> SumarEntradasKardexAsync(string documentoTipo, List<int> idsDocumento)
    {
        return _context.MovimientosKardex
            .Where(k => k.DocumentoTipo == documentoTipo && k.DocumentoId != null && idsDocumento.Contains(k.DocumentoId.Value))
            .GroupBy(k => k.ProductoId).Select(g => new { g.Key, C = g.Sum(x => x.CantidadEntrada) })
            .ToDictionaryAsync(x => x.Key, x => x.C);
    }

    private async Task ValidarProveedorActivoAsync(int proveedorId)
    {
        if (!await _context.Proveedores.AnyAsync(p => p.Id == proveedorId && p.Activo))
            throw new InvalidOperationException($"El proveedor {proveedorId} no existe o está inactivo.");
    }

    private async Task ValidarProductosAsync(IEnumerable<int> productoIds)
    {
        var ids = productoIds.Distinct().ToList();
        var existentes = await _context.Productos.Where(p => ids.Contains(p.Id) && p.Activo).Select(p => p.Id).ToListAsync();
        var faltantes = ids.Except(existentes).ToList();
        if (faltantes.Count > 0)
            throw new InvalidOperationException($"Productos inexistentes o inactivos: {string.Join(", ", faltantes)}.");
    }

    /// <inheritdoc />
    public Task<OrdenCompra> AnularOrdenAsync(int ordenId, string motivo, string usuario)
    {
        return _context.EnTransaccionAsync(async () =>
        {
            var orden = await _context.OrdenesCompra.Include(o => o.Detalles)
                .FirstOrDefaultAsync(o => o.Id == ordenId)
                ?? throw new KeyNotFoundException("Orden de compra no encontrada.");

            if (orden.Estado == EstadoOrdenCompra.Anulada)
                throw new InvalidOperationException("La orden ya está anulada.");
            if (orden.Detalles.Any(d => d.CantidadRecibida > 0) || await _context.FacturasCompra.AnyAsync(f => f.OrdenCompraId == ordenId))
                throw new InvalidOperationException("Solo se pueden anular órdenes sin recepciones ni facturas.");

            ExigirMotivo(motivo);
            orden.Estado = EstadoOrdenCompra.Anulada;
            orden.MotivoAnulacion = motivo.Trim();
            orden.FechaAnulacion = DateTime.UtcNow;
            orden.AnuladoPor = usuario;
            orden.Observaciones = $"{orden.Observaciones} | [ANULADA por {usuario}: {motivo}]";
            orden.ModificadoPor = usuario;
            orden.FechaModificacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return orden;
        });
    }

    /// <inheritdoc />
    public Task<OrdenCompra> AprobarOrdenAsync(int ordenId, string usuario)
    {
        return _context.EnTransaccionAsync(async () =>
        {
            var orden = await _context.OrdenesCompra
                .FirstOrDefaultAsync(o => o.Id == ordenId)
                ?? throw new KeyNotFoundException("Orden de compra no encontrada.");

            if (orden.Estado != EstadoOrdenCompra.Pendiente)
                throw new InvalidOperationException($"Solo se aprueban órdenes Pendientes; la orden {orden.NumeroOrden} está {orden.Estado}.");

            orden.Estado = EstadoOrdenCompra.Aprobada;
            orden.AprobadoPor = usuario;
            orden.FechaAprobacion = DateTime.UtcNow;
            orden.Observaciones = $"{orden.Observaciones} | [APROBADA por {usuario}]".TrimStart(' ', '|');
            orden.ModificadoPor = usuario;
            orden.FechaModificacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return orden;
        });
    }

    /// <inheritdoc />
    public async Task<List<RecepcionCompra>> GetRecepcionesAsync()
    {
        return await _context.RecepcionesCompra
            .Include(r => r.OrdenCompra).ThenInclude(o => o!.Proveedor)
            .Include(r => r.Detalles).ThenInclude(d => d.Producto)
            .AsNoTracking()
            .OrderByDescending(r => r.FechaRecepcion)
            .ToListAsync();
    }

    private static void ExigirMotivo(string? motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo) || motivo.Trim().Length < 3)
            throw new InvalidOperationException("El motivo de anulación es obligatorio (mínimo 3 caracteres).");
    }

    /// <inheritdoc />
    public Task<FacturaCompra> AnularFacturaAsync(int facturaId, string motivo, string usuario)
    {
        return _context.EnTransaccionAsync(async () =>
        {
            ExigirMotivo(motivo);

            var factura = await _context.FacturasCompra
                .Include(f => f.Detalles)
                .Include(f => f.Pagos)
                .FirstOrDefaultAsync(f => f.Id == facturaId)
                ?? throw new KeyNotFoundException("Factura de compra no encontrada.");

            if (factura.Estado == EstadoFacturaCompra.Anulada)
                throw new InvalidOperationException("La factura ya está anulada.");
            if (factura.Pagos.Any())
                throw new InvalidOperationException("La factura tiene pagos registrados; no puede anularse directamente.");

            // 1. El stock que ingresó la recepción se queda (la mercadería sigue en bodega; la reversa del asiento
            //    devuelve el neto a "Recepciones por facturar"). Solo sale el stock que la propia factura ingresó
            //    (facturas sin recepción previa o del flujo antiguo), al CPP vigente.
            var ingresadoPorEsta = await SumarEntradasKardexAsync(DocFactura, new List<int> { factura.Id });
            OrdenCompra? ordenFactura = factura.OrdenCompraId.HasValue
                ? await _context.OrdenesCompra.Include(o => o.Detalles).FirstOrDefaultAsync(o => o.Id == factura.OrdenCompraId.Value)
                : null;
            foreach (var detalle in factura.Detalles)
            {
                var producto = await _context.Productos.FindAsync(detalle.ProductoId);
                if (producto == null) continue;

                var directo = ingresadoPorEsta.GetValueOrDefault(detalle.ProductoId);
                var cantDirecta = Math.Min(detalle.Cantidad, directo);
                ingresadoPorEsta[detalle.ProductoId] = directo - cantDirecta;

                // Revierte el ajuste de CPP por diferencia de precio de la parte que venía de la recepción.
                var cantTransito = detalle.Cantidad - cantDirecta;
                var precioOc = ordenFactura?.Detalles.FirstOrDefault(d => d.ProductoId == detalle.ProductoId)?.PrecioUnitario;
                if (cantTransito > 0 && precioOc.HasValue && producto.StockActual > 0)
                {
                    var diferencia = cantTransito * (detalle.PrecioUnitario - precioOc.Value);
                    producto.CostoPromedioPonderado = Math.Max(0, producto.CostoPromedioPonderado - diferencia / producto.StockActual);
                }

                if (cantDirecta <= 0) continue;
                if (producto.StockActual < cantDirecta)
                    throw new InvalidOperationException(
                        $"No se puede anular: el stock de '{producto.Nombre}' ({producto.StockActual}) es menor que lo ingresado por la factura ({cantDirecta}).");

                var nuevoStock = producto.StockActual - cantDirecta;
                producto.StockActual = nuevoStock;

                await _context.MovimientosKardex.AddAsync(new MovimientoKardex
                {
                    ProductoId = producto.Id,
                    Fecha = DateTime.UtcNow,
                    TipoMovimiento = TipoMovimientoKardex.AjusteSalida,
                    DocumentoTipo = "AnulacionFacturaCompra",
                    DocumentoNumero = factura.NumeroFactura,
                    DocumentoId = factura.Id,
                    CantidadSalida = cantDirecta,
                    CostoUnitarioSalida = producto.CostoPromedioPonderado,
                    SaldoCantidad = nuevoStock,
                    CostoPromedioPonderadoResultante = producto.CostoPromedioPonderado,
                    SaldoValorizado = nuevoStock * producto.CostoPromedioPonderado,
                    Glosa = $"Anulación Factura Compra {factura.NumeroFactura}: {motivo.Trim()}",
                    CreadoPor = usuario
                });
            }

            // 2. Asiento de reversa (valida período abierto y enlaza AsientoRevertidoId).
            if (factura.AsientoContableId.HasValue)
                await _contabilidad.RevertirAsientoAsync(factura.AsientoContableId.Value,
                    $"Reversa factura compra {factura.NumeroFactura}: {motivo.Trim()}", usuario);

            factura.Estado = EstadoFacturaCompra.Anulada;
            factura.SaldoPendiente = 0;
            factura.MotivoAnulacion = motivo.Trim();
            factura.FechaAnulacion = DateTime.UtcNow;
            factura.AnuladoPor = usuario;
            factura.ModificadoPor = usuario;
            factura.FechaModificacion = DateTime.UtcNow;

            // 3. La OC deja de estar Liquidada: vuelve a recibida (total o parcial) para poder refacturarse.
            if (factura.OrdenCompraId.HasValue)
            {
                var orden = await _context.OrdenesCompra.Include(o => o.Detalles).FirstOrDefaultAsync(o => o.Id == factura.OrdenCompraId.Value);
                if (orden is { Estado: EstadoOrdenCompra.Liquidada })
                    orden.Estado = orden.Detalles.All(d => d.CantidadRecibida >= d.Cantidad)
                        ? EstadoOrdenCompra.RecibidaTotal : EstadoOrdenCompra.RecibidaParcial;
            }

            await _context.SaveChangesAsync();
            return factura;
        });
    }

    /// <inheritdoc />
    public Task<PagoProveedor> RegistrarPagoAsync(RegistrarPagoCompraDto dto, string usuario)
    {
        return _context.EnTransaccionAsync(async () =>
        {
            var metodoPago = MetodosPago.Normalizar(dto.MetodoPago);
            if (metodoPago == MetodosPago.Credito)
                throw new InvalidOperationException("Un pago a proveedor no puede registrarse con método Credito.");

            var factura = await _context.FacturasCompra
                .FirstOrDefaultAsync(f => f.Id == dto.CompraProveedorId)
                ?? throw new KeyNotFoundException("Factura de compra no encontrada.");

            if (factura.Estado != EstadoFacturaCompra.Aprobada)
                throw new InvalidOperationException($"Solo se pagan facturas Aprobadas; la factura {factura.NumeroFactura} está {factura.Estado}.");
            if (dto.Monto <= 0 || dto.Monto > factura.SaldoPendiente)
                throw new InvalidOperationException($"Monto de pago inválido. Saldo pendiente actual: {factura.SaldoPendiente}");

            var pago = new PagoProveedor
            {
                CompraProveedorId = factura.Id,
                FechaPago = DateTime.UtcNow,
                Monto = dto.Monto,
                MetodoPago = metodoPago,
                ReferenciaComprobante = dto.ReferenciaComprobante,
                CreadoPor = usuario
            };

            factura.TotalPagado += dto.Monto;
            factura.SaldoPendiente -= dto.Monto;
            factura.ModificadoPor = usuario;
            factura.FechaModificacion = DateTime.UtcNow;

            // Asiento de pago: Debe Proveedores (2.1.01.01) / Haber Caja o Banco [monto]. Las cuentas salen de la
            // matriz de integración contable (PagoProveedorEfectivo / PagoProveedorBanco) con las fijas como respaldo.
            var efectivo = MetodosPago.EntraACaja(metodoPago);
            var (codDebe, codHaber) = await _contabilidad.ResolverCuentasAsync(
                "Compras",
                efectivo ? MatrizIntegracionSemilla.PagoProveedorEfectivo : MatrizIntegracionSemilla.PagoProveedorBanco,
                CuentasContablesConst.Proveedores,
                efectivo ? CuentasContablesConst.Caja : CuentasContablesConst.Banco);

            var asientoPago = await _contabilidad.RegistrarAsientoAutomaticoAsync(new AsientoAutomaticoDto(
                $"Pago Factura Compra N° {factura.NumeroFactura} - Ref: {dto.ReferenciaComprobante}",
                "Compras", "PagoProveedor", factura.Id,
                new List<LineaAsientoAutomaticoDto>
                {
                    new(codDebe, dto.Monto, 0, "Cancelación Cuenta por Pagar Proveedor"),
                    new(codHaber, 0, dto.Monto, "Salida de Fondos por Pago")
                }), usuario);

            pago.AsientoPagoId = asientoPago.Id;

            await _context.PagosProveedor.AddAsync(pago);
            await _context.SaveChangesAsync();
            return pago;
        });
    }

    /// <inheritdoc />
    public async Task<List<PagoProveedor>> GetPagosAsync(int? facturaId)
    {
        var query = _context.PagosProveedor
            .Include(p => p.FacturaCompra).ThenInclude(f => f!.Proveedor)
            .AsNoTracking().AsQueryable();
        if (facturaId.HasValue) query = query.Where(p => p.CompraProveedorId == facturaId.Value);
        return await query.OrderByDescending(p => p.FechaPago).ToListAsync();
    }
}
