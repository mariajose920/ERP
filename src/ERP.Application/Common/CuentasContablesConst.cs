// Módulo: Contabilidad (transversal T7) — Responsable: Cristóbal
namespace ERP.Application.Common;

/// <summary>
/// Códigos del plan de cuentas usados por los asientos automáticos de Compras, Ventas e Inventario.
/// Deben coincidir con el seed de DbInitializer. Antes estaban repetidos como literales en 3 servicios.
/// </summary>
public static class CuentasContablesConst
{
    /// <summary>Caja General (activo, deudora).</summary>
    public const string Caja = "1.1.01.01";
    /// <summary>Banco Principal (activo, deudora).</summary>
    public const string Banco = "1.1.01.02";
    /// <summary>Clientes Nacionales / Cuentas por cobrar (activo, deudora).</summary>
    public const string Clientes = "1.1.02.01";
    /// <summary>Inventario de Mercaderías (activo, deudora).</summary>
    public const string Inventario = "1.1.03.01";
    /// <summary>IVA Crédito Fiscal (activo, deudora).</summary>
    public const string IvaCreditoFiscal = "1.1.04.01";
    /// <summary>Proveedores Nacionales / Cuentas por pagar (pasivo, acreedora).</summary>
    public const string Proveedores = "2.1.01.01";
    /// <summary>IVA Débito Fiscal (pasivo, acreedora).</summary>
    public const string IvaDebitoFiscal = "2.1.02.01";
    /// <summary>Ingresos por Ventas (ingreso, acreedora).</summary>
    public const string IngresosVentas = "4.1.01.01";
    /// <summary>Costo de Ventas (costo, deudora).</summary>
    public const string CostoVentas = "5.1.01.01";
    /// <summary>Gastos Operacionales / Mermas (gasto, deudora).</summary>
    public const string GastosMermas = "6.1.01.01";
}

/// <summary>Parámetros tributarios (T7).</summary>
public static class ImpuestosConst
{
    /// <summary>Tasa de IVA vigente en Chile.</summary>
    public const decimal TasaIva = 0.19m;

    /// <summary>Calcula el IVA redondeado a 2 decimales.</summary>
    public static decimal CalcularIva(decimal montoNeto) => Math.Round(montoNeto * TasaIva, 2);

    /// <summary>
    /// Neto contenido en un monto con IVA incluido (precio de venta al público): bruto / 1,19, redondeado a 2 decimales.
    /// El IVA es la diferencia (bruto − neto), de modo que neto + IVA = bruto exacto.
    /// </summary>
    public static decimal NetoDesdeBruto(decimal montoBruto) => Math.Round(montoBruto / (1 + TasaIva), 2);
}

/// <summary>Cliente genérico para ventas sin cliente identificado (boleta a consumidor final).</summary>
public static class ClientesConst
{
    public const string RutConsumidorFinal = "66.666.666-6";
    public const string NombreConsumidorFinal = "CONSUMIDOR FINAL";
}

/// <summary>Comisión de vendedores. Se guarda por usuario (Usuarios.PorcentajeComision); si es nula se usa este valor.</summary>
public static class ComisionesConst
{
    public const decimal PorcentajePorDefecto = 2m;
}

/// <summary>
/// Claves (ModuloOrigen, TipoOperacion) de la matriz de integración contable (tabla MatrizIntegracionContable)
/// y su semilla, derivada de las cuentas fijas de arriba.
/// </summary>
public static class MatrizIntegracionSemilla
{
    public const string PagoProveedorEfectivo = "PagoProveedorEfectivo";
    public const string PagoProveedorBanco = "PagoProveedorBanco";

    /// <summary>(Módulo, Operación, Código cuenta Debe, Código cuenta Haber, Descripción).</summary>
    public static readonly IReadOnlyList<(string Modulo, string Operacion, string Debe, string Haber, string Descripcion)> Filas = new[]
    {
        ("Compras", "FacturaCompraNeto", CuentasContablesConst.Inventario, CuentasContablesConst.Proveedores, "Factura de compra: ingreso de mercadería (neto) contra cuenta por pagar"),
        ("Compras", "FacturaCompraIva", CuentasContablesConst.IvaCreditoFiscal, CuentasContablesConst.Proveedores, "Factura de compra: IVA crédito fiscal contra cuenta por pagar"),
        ("Compras", PagoProveedorEfectivo, CuentasContablesConst.Proveedores, CuentasContablesConst.Caja, "Pago a proveedor en efectivo (sale de Caja)"),
        ("Compras", PagoProveedorBanco, CuentasContablesConst.Proveedores, CuentasContablesConst.Banco, "Pago a proveedor por transferencia o tarjeta (sale de Banco)"),
        ("Ventas", "VentaContadoEfectivo", CuentasContablesConst.Caja, CuentasContablesConst.IngresosVentas, "Venta al contado en efectivo: cobro contra ingreso (neto)"),
        ("Ventas", "VentaContadoBanco", CuentasContablesConst.Banco, CuentasContablesConst.IngresosVentas, "Venta con tarjeta/transferencia: cobro contra ingreso (neto)"),
        ("Ventas", "VentaCredito", CuentasContablesConst.Clientes, CuentasContablesConst.IngresosVentas, "Venta a crédito: cuenta por cobrar contra ingreso (neto)"),
        ("Ventas", "IvaDebitoVenta", CuentasContablesConst.Clientes, CuentasContablesConst.IvaDebitoFiscal, "IVA débito de la venta (el Debe real es la cuenta de cobro de la venta: Caja, Banco o Clientes)"),
        ("Ventas", "CostoVenta", CuentasContablesConst.CostoVentas, CuentasContablesConst.Inventario, "Costo de ventas (CPP) contra salida de inventario"),
        ("Ventas", "AbonoClienteEfectivo", CuentasContablesConst.Caja, CuentasContablesConst.Clientes, "Abono de cliente en efectivo"),
        ("Ventas", "AbonoClienteBanco", CuentasContablesConst.Banco, CuentasContablesConst.Clientes, "Abono de cliente por transferencia o tarjeta"),
        ("Inventario", "AjusteMerma", CuentasContablesConst.GastosMermas, CuentasContablesConst.Inventario, "Ajuste de inventario negativo (merma/pérdida)"),
        ("Inventario", "AjusteSobrante", CuentasContablesConst.Inventario, CuentasContablesConst.GastosMermas, "Ajuste de inventario positivo (sobrante)")
    };
}
