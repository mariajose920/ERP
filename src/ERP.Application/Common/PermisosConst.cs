// Módulo: Seguridad (RBAC, K5) — Responsable: Cristóbal
namespace ERP.Application.Common;

/// <summary>
/// Códigos de permiso. Cada código es también el nombre de una política de autorización
/// (<c>[Authorize(Policy = PermisosConst.ComprasOrdenesEscribir)]</c>). El rol Administrador
/// cumple todas las políticas.
/// </summary>
public static class PermisosConst
{
    public const string ComprasProveedoresLeer = "compras.proveedores.leer";
    public const string ComprasProveedoresEscribir = "compras.proveedores.escribir";
    public const string ComprasOrdenesLeer = "compras.ordenes.leer";
    public const string ComprasOrdenesEscribir = "compras.ordenes.escribir";
    public const string ComprasRecepcionesCrear = "compras.recepciones.crear";
    public const string ComprasFacturasCrear = "compras.facturas.crear";
    public const string ComprasPagosCrear = "compras.pagos.crear";

    public const string VentasClientesLeer = "ventas.clientes.leer";
    public const string VentasClientesEscribir = "ventas.clientes.escribir";
    public const string VentasDocumentosLeer = "ventas.documentos.leer";
    public const string VentasDocumentosCrear = "ventas.documentos.crear";
    public const string VentasDocumentosAnular = "ventas.documentos.anular";
    public const string VentasPagosCrear = "ventas.pagos.crear";
    public const string VentasProductosConsultar = "ventas.productos.consultar";
    public const string VentasClientesCrear = "ventas.clientes.crear";
    public const string VentasCajaPropias = "ventas.caja.propias";

    public const string InventarioProductosEscribir = "inventario.productos.escribir";
    public const string InventarioStockLeer = "inventario.stock.leer";
    public const string InventarioAjustesCrear = "inventario.ajustes.crear";

    public const string ContabilidadAsientosLeer = "contabilidad.asientos.leer";
    public const string ContabilidadAsientosCrear = "contabilidad.asientos.crear";
    public const string ContabilidadReportesLeer = "contabilidad.reportes.leer";
    public const string ContabilidadCuentasEscribir = "contabilidad.cuentas.escribir";
    public const string ContabilidadPeriodosAdministrar = "contabilidad.periodos.administrar";

    public const string UsuariosAdministrar = "usuarios.administrar";

    /// <summary>Nombre del rol de caja (perfil restringido).</summary>
    public const string RolCajero = "Cajero";

    /// <summary>Nombre del rol con acceso total.</summary>
    public const string RolAdministrador = "Administrador";

    /// <summary>
    /// Permisos EXACTOS del perfil Cajero: vender en POS, consultar productos/precios/stock,
    /// consultar, crear y editar clientes (incluido su límite de crédito), registrar abonos y ver sus
    /// ventas del día. Sin compras, inventario, contabilidad, reportes ni administración.
    /// </summary>
    public static readonly IReadOnlyList<string> PermisosCajero = new[]
    {
        VentasDocumentosCrear, VentasProductosConsultar, VentasClientesLeer,
        VentasClientesCrear, VentasClientesEscribir, VentasPagosCrear, VentasCajaPropias
    };

    /// <summary>Todos los códigos (para registrar políticas y completar el seed).</summary>
    public static readonly IReadOnlyList<(string Codigo, string Modulo, string Descripcion)> Todos = new[]
    {
        (ComprasProveedoresLeer, "Compras", "Ver proveedores"),
        (ComprasProveedoresEscribir, "Compras", "Crear/Editar proveedores"),
        (ComprasOrdenesLeer, "Compras", "Ver órdenes de compra"),
        (ComprasOrdenesEscribir, "Compras", "Crear/Editar órdenes de compra"),
        (ComprasRecepcionesCrear, "Compras", "Registrar recepciones de mercadería"),
        (ComprasFacturasCrear, "Compras", "Registrar y aprobar facturas de compra"),
        (ComprasPagosCrear, "Compras", "Registrar pagos a proveedores"),
        (VentasClientesLeer, "Ventas", "Ver clientes"),
        (VentasClientesEscribir, "Ventas", "Crear/Editar clientes"),
        (VentasDocumentosLeer, "Ventas", "Ver ventas y reportes de ventas"),
        (VentasDocumentosCrear, "Ventas", "Emitir ventas/facturas"),
        (VentasDocumentosAnular, "Ventas", "Anular ventas emitidas"),
        (VentasPagosCrear, "Ventas", "Registrar abonos de clientes"),
        (VentasProductosConsultar, "Ventas", "Consultar productos, precios y stock para vender (POS)"),
        (VentasClientesCrear, "Ventas", "Crear clientes básicos desde caja"),
        (VentasCajaPropias, "Ventas", "Ver las ventas propias del día (cierre de caja)"),
        (InventarioProductosEscribir, "Inventario", "Crear/Editar productos"),
        (InventarioStockLeer, "Inventario", "Consultar stock y kardex"),
        (InventarioAjustesCrear, "Inventario", "Registrar ajustes de inventario"),
        (ContabilidadAsientosLeer, "Contabilidad", "Ver asientos contables y libros"),
        (ContabilidadAsientosCrear, "Contabilidad", "Crear asientos contables"),
        (ContabilidadReportesLeer, "Contabilidad", "Ver balances y estado de resultados"),
        (ContabilidadCuentasEscribir, "Contabilidad", "Crear cuentas del plan de cuentas"),
        (ContabilidadPeriodosAdministrar, "Contabilidad", "Cerrar y reabrir períodos contables"),
        (UsuariosAdministrar, "Seguridad", "Administrar usuarios y roles")
    };
}
