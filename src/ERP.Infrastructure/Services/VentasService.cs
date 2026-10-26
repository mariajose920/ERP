// Módulo: Ventas — Responsable: María
using ERP.Application.Common;
using ERP.Application.DTOs;
using ERP.Application.Interfaces;
using ERP.Domain.Entities.Inventario;
using ERP.Domain.Entities.Ventas;
using ERP.Domain.Enums;
using ERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Services;

/// <summary>
/// Emisión, anulación y cobranza de ventas.
/// <para>Métodos de pago: Efectivo (entra a Caja), Transferencia y Tarjeta (entran a Banco) quedan
/// pagadas al emitir; Credito deja el saldo en Clientes (cuentas por cobrar) con vencimiento a
/// <see cref="DiasCreditoPorDefecto"/> días y respeta el límite de crédito del cliente.</para>
/// </summary>
public class VentasService : IVentasService
{
    /// <summary>Plazo de pago de las ventas a crédito (M4).</summary>
    public const int DiasCreditoPorDefecto = 30;

    private readonly ErpDbContext _context;
    private readonly INumeradorDocumentos _numerador;
    private readonly IContabilidadService _contabilidad;

    public VentasService(ErpDbContext context, INumeradorDocumentos numerador, IContabilidadService contabilidad)
    {
        _context = context;
        _numerador = numerador;
        _contabilidad = contabilidad;
    }

    /// <inheritdoc />
    public Task<Venta> EmitirVentaAsync(CrearVentaDto dto, string usuario)
    {
        return _context.EnTransaccionAsync(async () =>
        {
            var metodoPago = MetodosPago.Normalizar(dto.MetodoPago); // M5
            var esContado = MetodosPago.EsContado(metodoPago);

            // Sin cliente identificado la venta se asocia al cliente genérico "Consumidor final" (solo contado).
            var cliente = dto.ClienteId is > 0
                ? await _context.Clientes.FindAsync(dto.ClienteId.Value) ?? throw new KeyNotFoundException("Cliente no encontrado.")
                : await ObtenerConsumidorFinalAsync(usuario);
            if (!cliente.Activo)
                throw new InvalidOperationException($"El cliente {cliente.RazonSocial} está inactivo."); // M7

            // M1: el número se obtiene ANTES de crear los movimientos de kardex, que lo reciben directamente.
            var numeroFactura = await _numerador.SiguienteAsync(TipoDocumentoNumerado.FacturaVenta);

            decimal subtotal = 0, totalBruto = 0, totalDescuento = 0, costoVentaTotal = 0;
            var detalles = new List<DetalleVenta>();
            var movimientos = new List<MovimientoKardex>();

            // 1. Validación de stock/descuentos, descuento de stock y kardex.
            foreach (var item in dto.Items)
            {
                var producto = await _context.Productos.FindAsync(item.ProductoId)
                    ?? throw new KeyNotFoundException($"Producto {item.ProductoId} no encontrado.");

                if (!producto.Activo)
                    throw new InvalidOperationException($"El producto '{producto.Nombre}' está inactivo.");
                if (item.Cantidad <= 0)
                    throw new InvalidOperationException($"La cantidad de '{producto.Nombre}' debe ser mayor a 0."); // M7
                if (item.Descuento < 0 || item.Descuento > item.Cantidad * item.PrecioUnitario)
                    throw new InvalidOperationException($"El descuento de '{producto.Nombre}' no puede superar Cantidad × Precio."); // M7
                if (producto.StockActual < item.Cantidad)
                    throw new InvalidOperationException($"Stock insuficiente para '{producto.Nombre}'. Disponible: {producto.StockActual}, Solicitado: {item.Cantidad}");

                // El precio de venta incluye IVA: el monto de la línea es bruto y el neto se obtiene dividiendo por 1,19.
                var itemBruto = (item.Cantidad * item.PrecioUnitario) - item.Descuento;
                var itemSubtotal = ImpuestosConst.NetoDesdeBruto(itemBruto);
                subtotal += itemSubtotal;
                totalBruto += itemBruto;
                totalDescuento += item.Descuento;
                costoVentaTotal += item.Cantidad * producto.CostoPromedioPonderado;

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

                var nuevoStock = producto.StockActual - item.Cantidad;
                producto.StockActual = nuevoStock;

                var kardex = new MovimientoKardex
                {
                    ProductoId = producto.Id,
                    Fecha = DateTime.UtcNow,
                    TipoMovimiento = TipoMovimientoKardex.SalidaVenta,
                    DocumentoTipo = "FacturaVenta",
                    DocumentoNumero = numeroFactura,
                    CantidadSalida = item.Cantidad,
                    CostoUnitarioSalida = producto.CostoPromedioPonderado,
                    SaldoCantidad = nuevoStock,
                    CostoPromedioPonderadoResultante = producto.CostoPromedioPonderado,
                    SaldoValorizado = nuevoStock * producto.CostoPromedioPonderado,
                    Glosa = $"Venta a {cliente.RazonSocial}",
                    CreadoPor = usuario
                };
                movimientos.Add(kardex);
                await _context.MovimientosKardex.AddAsync(kardex);
            }

            // Total = lo que paga el cliente (suma de precios con IVA incluido); el IVA es la parte contenida en él.
            var total = totalBruto;
            var iva = total - subtotal;

            // M3: límite de crédito (solo ventas a crédito).
            if (!esContado)
            {
                if (cliente.LimiteCredito <= 0)
                    throw new InvalidOperationException($"El cliente {cliente.RazonSocial} no tiene línea de crédito asignada.");
                if (cliente.SaldoPendiente + total > cliente.LimiteCredito)
                    throw new InvalidOperationException(
                        $"La venta excede el límite de crédito del cliente. Límite: {cliente.LimiteCredito:N0}, deuda actual: {cliente.SaldoPendiente:N0}, venta: {total:N0}.");
            }

            var fechaEmision = DateTime.UtcNow;
            var venta = new Venta
            {
                NumeroFactura = numeroFactura,
                FechaEmision = fechaEmision,
                FechaVencimiento = esContado ? fechaEmision : fechaEmision.AddDays(DiasCreditoPorDefecto), // M4
                ClienteId = cliente.Id,
                Estado = esContado ? EstadoVenta.Pagada : EstadoVenta.Emitida,
                Subtotal = subtotal,
                ImpuestoIva = iva,
                Descuento = totalDescuento,
                Total = total,
                TotalPagado = esContado ? total : 0,
                SaldoPendiente = esContado ? 0 : total,
                CostoVentaTotal = costoVentaTotal,
                MetodoPago = metodoPago,
                Observaciones = dto.Observaciones,
                CreadoPor = usuario,
                Detalles = detalles
            };

            await _context.Ventas.AddAsync(venta);
            await _context.SaveChangesAsync();

            // M1: se enlazan SOLO los movimientos creados por esta venta.
            foreach (var k in movimientos) k.DocumentoId = venta.Id;

            // 2. Asientos automáticos.
            // (a) Asiento de venta:
            //     Debe  Caja (1.1.01.01) si es efectivo, Banco (1.1.01.02) si es transferencia/tarjeta,
            //           o Clientes (1.1.02.01) si es a crédito                       [total]
            //     Haber Ingresos por Ventas (4.1.01.01)                               [neto]
            //     Haber IVA Débito Fiscal (2.1.02.01)                                 [IVA]
            var cuentaCobro = !esContado
                ? CuentasContablesConst.Clientes
                : MetodosPago.EntraACaja(metodoPago) ? CuentasContablesConst.Caja : CuentasContablesConst.Banco;

            var asientoVenta = await _contabilidad.RegistrarAsientoAutomaticoAsync(new AsientoAutomaticoDto(
                $"Venta Factura N° {venta.NumeroFactura} - {cliente.RazonSocial}",
                "Ventas", "FacturaVenta", venta.Id,
                new List<LineaAsientoAutomaticoDto>
                {
                    new(cuentaCobro, total, 0, esContado ? "Cobro de la venta" : "Cuenta por cobrar cliente"),
                    new(CuentasContablesConst.IngresosVentas, 0, subtotal, "Ingreso por Ventas"),
                    new(CuentasContablesConst.IvaDebitoFiscal, 0, iva, "IVA Débito Fiscal")
                }), usuario);
            venta.AsientoVentaId = asientoVenta.Id;

            // (b) Asiento de costo: Debe Costo de Ventas (5.1.01.01) / Haber Inventario (1.1.03.01) [costo CPP]
            if (costoVentaTotal > 0)
            {
                var asientoCosto = await _contabilidad.RegistrarAsientoAutomaticoAsync(new AsientoAutomaticoDto(
                    $"Costo de Venta Factura N° {venta.NumeroFactura}",
                    "Ventas", "CostoVenta", venta.Id,
                    new List<LineaAsientoAutomaticoDto>
                    {
                        new(CuentasContablesConst.CostoVentas, costoVentaTotal, 0, "Costo de Ventas Reconocido"),
                        new(CuentasContablesConst.Inventario, 0, costoVentaTotal, "Salida de Mercadería por Venta")
                    }), usuario);
                venta.AsientoCostoVentaId = asientoCosto.Id;
            }

            if (venta.SaldoPendiente > 0)
                cliente.SaldoPendiente += venta.SaldoPendiente;

            await _context.SaveChangesAsync();
            return venta;
        });
    }

    /// <summary>Obtiene (o crea) el cliente genérico para ventas sin cliente identificado.</summary>
    private async Task<ERP.Domain.Entities.Maestros.Cliente> ObtenerConsumidorFinalAsync(string usuario)
    {
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Rut == ClientesConst.RutConsumidorFinal);
        if (cliente == null)
        {
            cliente = new ERP.Domain.Entities.Maestros.Cliente
            {
                Rut = ClientesConst.RutConsumidorFinal,
                RazonSocial = ClientesConst.NombreConsumidorFinal,
                Email = "consumidor.final@erp.local",
                LimiteCredito = 0,
                Activo = true,
                CreadoPor = usuario
            };
            await _context.Clientes.AddAsync(cliente);
            await _context.SaveChangesAsync();
        }
        return cliente;
    }

    /// <inheritdoc />
    public Task<Venta> AnularVentaAsync(int ventaId, string motivo, string usuario)
    {
        return _context.EnTransaccionAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(motivo) || motivo.Trim().Length < 3)
                throw new InvalidOperationException("El motivo de anulación es obligatorio (mínimo 3 caracteres).");

            var venta = await _context.Ventas
                .Include(v => v.Detalles)
                .Include(v => v.Cliente)
                .Include(v => v.Pagos)
                .FirstOrDefaultAsync(v => v.Id == ventaId)
                ?? throw new KeyNotFoundException("Venta no encontrada.");

            if (venta.Estado == EstadoVenta.Anulada)
                throw new InvalidOperationException("La venta ya está anulada.");

            // Con abonos registrados la reversa dejaría la cuenta por cobrar inconsistente:
            // primero debe gestionarse la devolución del dinero.
            if (venta.Pagos.Any())
                throw new InvalidOperationException("La venta tiene abonos registrados; no puede anularse directamente.");

            // 1. Reposición de stock y kardex de devolución (al costo con que salió).
            foreach (var detalle in venta.Detalles)
            {
                var producto = await _context.Productos.FindAsync(detalle.ProductoId);
                if (producto == null) continue;

                var nuevoStock = producto.StockActual + detalle.Cantidad;
                var nuevoCpp = InventarioService.CalcularCpp(producto.StockActual, producto.CostoPromedioPonderado, detalle.Cantidad, detalle.CostoUnitarioSnapshot);
                producto.StockActual = nuevoStock;
                producto.CostoPromedioPonderado = nuevoCpp;

                await _context.MovimientosKardex.AddAsync(new MovimientoKardex
                {
                    ProductoId = producto.Id,
                    Fecha = DateTime.UtcNow,
                    TipoMovimiento = TipoMovimientoKardex.Devolucion,
                    DocumentoTipo = "AnulacionVenta",
                    DocumentoNumero = venta.NumeroFactura,
                    DocumentoId = venta.Id,
                    CantidadEntrada = detalle.Cantidad,
                    CostoUnitarioEntrada = detalle.CostoUnitarioSnapshot,
                    SaldoCantidad = nuevoStock,
                    CostoPromedioPonderadoResultante = nuevoCpp,
                    SaldoValorizado = nuevoStock * nuevoCpp,
                    Glosa = $"Anulación Venta: {motivo}",
                    CreadoPor = usuario
                });
            }

            // 2. M2: asientos de reversa de la venta y del costo.
            if (venta.AsientoVentaId.HasValue)
                await _contabilidad.RevertirAsientoAsync(venta.AsientoVentaId.Value, $"Reversa venta {venta.NumeroFactura}: {motivo}", usuario);
            if (venta.AsientoCostoVentaId.HasValue)
                await _contabilidad.RevertirAsientoAsync(venta.AsientoCostoVentaId.Value, $"Reversa costo venta {venta.NumeroFactura}: {motivo}", usuario);

            // 3. Deuda del cliente.
            if (venta.Cliente != null && venta.SaldoPendiente > 0)
                venta.Cliente.SaldoPendiente = Math.Max(0, venta.Cliente.SaldoPendiente - venta.SaldoPendiente);

            venta.Estado = EstadoVenta.Anulada;
            venta.MotivoAnulacion = motivo.Trim();
            venta.FechaAnulacion = DateTime.UtcNow;
            venta.AnuladoPor = usuario;
            venta.SaldoPendiente = 0;
            venta.Observaciones = $"{venta.Observaciones} | [ANULADA por {usuario}: {motivo}]";
            venta.ModificadoPor = usuario;
            venta.FechaModificacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return venta;
        });
    }

    /// <inheritdoc />
    public Task<PagoVenta> RegistrarPagoAsync(RegistrarPagoVentaDto dto, string usuario)
    {
        return _context.EnTransaccionAsync(async () =>
        {
            var metodoPago = MetodosPago.Normalizar(dto.MetodoPago);
            if (metodoPago == MetodosPago.Credito)
                throw new InvalidOperationException("Un abono no puede registrarse con método 'Credito'.");

            var venta = await _context.Ventas
                .Include(v => v.Cliente)
                .FirstOrDefaultAsync(v => v.Id == dto.VentaId)
                ?? throw new KeyNotFoundException("Venta no encontrada.");

            if (venta.Estado == EstadoVenta.Anulada)
                throw new InvalidOperationException("No se pueden registrar abonos en una venta anulada.");
            if (dto.Monto <= 0 || dto.Monto > venta.SaldoPendiente)
                throw new InvalidOperationException($"Monto de pago inválido. Saldo pendiente actual: {venta.SaldoPendiente}");

            var pago = new PagoVenta
            {
                VentaId = venta.Id,
                FechaPago = DateTime.UtcNow,
                Monto = dto.Monto,
                MetodoPago = metodoPago,
                ReferenciaComprobante = dto.ReferenciaComprobante,
                CreadoPor = usuario
            };

            venta.TotalPagado += dto.Monto;
            venta.SaldoPendiente -= dto.Monto;
            venta.Estado = venta.SaldoPendiente == 0 ? EstadoVenta.Pagada : EstadoVenta.ParcialmentePagada;

            if (venta.Cliente != null)
                venta.Cliente.SaldoPendiente = Math.Max(0, venta.Cliente.SaldoPendiente - dto.Monto);

            // Asiento de abono: Debe Caja/Banco [monto] / Haber Clientes (1.1.02.01) [monto].
            // El abono en cascada (repartir un monto entre varias facturas, de la más antigua a la más
            // nueva) lo arma la pantalla de cobranzas llamando a este endpoint una vez por factura.
            var asientoPago = await _contabilidad.RegistrarAsientoAutomaticoAsync(new AsientoAutomaticoDto(
                $"Pago Factura N° {venta.NumeroFactura} - Ref: {dto.ReferenciaComprobante}",
                "Ventas", "Pago", venta.Id,
                new List<LineaAsientoAutomaticoDto>
                {
                    new(MetodosPago.EntraACaja(metodoPago) ? CuentasContablesConst.Caja : CuentasContablesConst.Banco, dto.Monto, 0, "Ingreso Fondos por Pago"),
                    new(CuentasContablesConst.Clientes, 0, dto.Monto, "Abono a Cuenta por Cobrar")
                }), usuario);

            pago.AsientoPagoId = asientoPago.Id;

            await _context.PagosVenta.AddAsync(pago);
            await _context.SaveChangesAsync();
            return pago;
        });
    }

    /// <inheritdoc />
    public async Task<List<Venta>> GetVentasAsync()
    {
        return await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .Include(v => v.Pagos)
            .AsNoTracking()
            .OrderByDescending(v => v.FechaEmision)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<VentaPendienteDto>> GetVentasPendientesAsync(int? clienteId)
    {
        var query = _context.Ventas.AsNoTracking()
            .Where(v => v.SaldoPendiente > 0 && v.Estado != EstadoVenta.Anulada);
        if (clienteId.HasValue) query = query.Where(v => v.ClienteId == clienteId.Value);

        var filas = await query
            .OrderBy(v => v.FechaEmision)
            .Select(v => new
            {
                v.Id, v.NumeroFactura, v.FechaEmision, v.FechaVencimiento, v.ClienteId,
                Cliente = v.Cliente!.RazonSocial, ClienteRut = v.Cliente.Rut,
                v.Total, v.TotalPagado, v.SaldoPendiente
            })
            .ToListAsync();

        var hoy = DateTime.UtcNow;
        return filas.Select(v => new VentaPendienteDto(
            v.Id, v.NumeroFactura, v.FechaEmision, v.FechaVencimiento, v.ClienteId, v.Cliente, v.ClienteRut,
            v.Total, v.TotalPagado, v.SaldoPendiente,
            v.FechaVencimiento.HasValue && hoy > v.FechaVencimiento.Value ? (int)(hoy - v.FechaVencimiento.Value).TotalDays : 0
        )).ToList();
    }

    /// <inheritdoc />
    public Task<AbonoCascadaResultadoDto> RegistrarAbonoCascadaAsync(AbonoCascadaDto dto, string usuario)
    {
        return _context.EnTransaccionAsync(async () =>
        {
            // Abono en cascada: se paga primero la venta más antigua; cada tramo es un pago normal
            // (con su propio asiento Debe Caja/Banco / Haber Clientes) registrado por RegistrarPagoAsync.
            var pendientes = await _context.Ventas
                .Where(v => v.ClienteId == dto.ClienteId && v.SaldoPendiente > 0 && v.Estado != EstadoVenta.Anulada)
                .OrderBy(v => v.FechaEmision).ThenBy(v => v.Id)
                .Select(v => new { v.Id, v.NumeroFactura, v.SaldoPendiente })
                .ToListAsync();

            if (pendientes.Count == 0)
                throw new InvalidOperationException("El cliente no tiene ventas con saldo pendiente.");

            var restante = dto.Monto;
            var aplicados = new List<PagoAplicadoDto>();
            foreach (var v in pendientes)
            {
                if (restante <= 0) break;
                var monto = Math.Min(restante, v.SaldoPendiente);
                await RegistrarPagoAsync(new RegistrarPagoVentaDto(v.Id, monto, dto.MetodoPago, dto.ReferenciaComprobante), usuario);
                restante -= monto;
                aplicados.Add(new PagoAplicadoDto(v.Id, v.NumeroFactura, monto, v.SaldoPendiente - monto));
            }

            return new AbonoCascadaResultadoDto(aplicados, dto.Monto - restante, restante);
        });
    }

    /// <inheritdoc />
    public async Task<ResumenCajaDto> GetResumenCajaAsync(string usuario)
    {
        // "Hoy" según la hora local del servidor, expresado en UTC (las fechas se guardan en UTC).
        var inicioDia = DateTime.Today.ToUniversalTime();

        var ventas = await _context.Ventas.AsNoTracking()
            .Where(v => v.CreadoPor == usuario && v.FechaEmision >= inicioDia)
            .OrderByDescending(v => v.FechaEmision)
            .Select(v => new VentaCajaDto(v.Id, v.NumeroFactura, v.FechaEmision, v.Cliente!.RazonSocial,
                v.MetodoPago, v.Estado.ToString(), v.Total))
            .ToListAsync();

        var abonos = await _context.PagosVenta.AsNoTracking()
            .Where(p => p.CreadoPor == usuario && p.FechaPago >= inicioDia)
            .Select(p => p.Monto)
            .ToListAsync();

        var vigentes = ventas.Where(v => v.Estado != nameof(EstadoVenta.Anulada)).ToList();
        return new ResumenCajaDto(
            vigentes.Count,
            vigentes.Sum(v => v.Total),
            vigentes.Where(v => v.MetodoPago != MetodosPago.Credito).Sum(v => v.Total),
            vigentes.Where(v => v.MetodoPago == MetodosPago.Credito).Sum(v => v.Total),
            abonos.Count, abonos.Sum(), ventas);
    }
}
