using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarPagosProveedoresPeriodosMatriz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnuladoPor",
                table: "VentasClientes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAnulacion",
                table: "VentasClientes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoAnulacion",
                table: "VentasClientes",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PorcentajeComision",
                table: "Usuarios",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnuladoPor",
                table: "OrdenesCompraProveedor",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AprobadoPor",
                table: "OrdenesCompraProveedor",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAnulacion",
                table: "OrdenesCompraProveedor",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAprobacion",
                table: "OrdenesCompraProveedor",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoAnulacion",
                table: "OrdenesCompraProveedor",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnuladoPor",
                table: "ComprasProveedores",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAnulacion",
                table: "ComprasProveedores",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoAnulacion",
                table: "ComprasProveedores",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SaldoPendiente",
                table: "ComprasProveedores",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPagado",
                table: "ComprasProveedores",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "AsientoRevertidoId",
                table: "AsientosContables",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MatrizIntegracionContable",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModuloOrigen = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TipoOperacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CuentaDebeId = table.Column<int>(type: "int", nullable: false),
                    CuentaHaberId = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatrizIntegracionContable", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatrizIntegracionContable_CuentasContables_CuentaDebeId",
                        column: x => x.CuentaDebeId,
                        principalTable: "CuentasContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MatrizIntegracionContable_CuentasContables_CuentaHaberId",
                        column: x => x.CuentaHaberId,
                        principalTable: "CuentasContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PagosProveedores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompraProveedorId = table.Column<int>(type: "int", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MetodoPago = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReferenciaComprobante = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AsientoPagoId = table.Column<int>(type: "int", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagosProveedores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PagosProveedores_AsientosContables_AsientoPagoId",
                        column: x => x.AsientoPagoId,
                        principalTable: "AsientosContables",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PagosProveedores_ComprasProveedores_CompraProveedorId",
                        column: x => x.CompraProveedorId,
                        principalTable: "ComprasProveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PeriodosContables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Anio = table.Column<int>(type: "int", nullable: false),
                    Mes = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CerradoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodosContables", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AsientosContables_AsientoRevertidoId",
                table: "AsientosContables",
                column: "AsientoRevertidoId");

            migrationBuilder.CreateIndex(
                name: "IX_MatrizIntegracionContable_CuentaDebeId",
                table: "MatrizIntegracionContable",
                column: "CuentaDebeId");

            migrationBuilder.CreateIndex(
                name: "IX_MatrizIntegracionContable_CuentaHaberId",
                table: "MatrizIntegracionContable",
                column: "CuentaHaberId");

            migrationBuilder.CreateIndex(
                name: "IX_MatrizIntegracionContable_ModuloOrigen_TipoOperacion",
                table: "MatrizIntegracionContable",
                columns: new[] { "ModuloOrigen", "TipoOperacion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PagosProveedores_AsientoPagoId",
                table: "PagosProveedores",
                column: "AsientoPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_PagosProveedores_CompraProveedorId",
                table: "PagosProveedores",
                column: "CompraProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodosContables_Anio_Mes",
                table: "PeriodosContables",
                columns: new[] { "Anio", "Mes" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AsientosContables_AsientosContables_AsientoRevertidoId",
                table: "AsientosContables",
                column: "AsientoRevertidoId",
                principalTable: "AsientosContables",
                principalColumn: "Id");

            // Datos existentes: cuentas por pagar. Las facturas vigentes parten con SaldoPendiente = Total
            // (sin pagos registrados); las anuladas no tienen saldo (EstadoFacturaCompra.Anulada = 3).
            EjecutarSql(migrationBuilder,
                "UPDATE ComprasProveedores SET SaldoPendiente = CASE WHEN Estado = 3 THEN 0 ELSE Total END, TotalPagado = 0;");

            // Órdenes ya aprobadas antes de existir AprobadoPor: se deja sin completar (no hay forma confiable de inferirlo).

            // Semilla de la matriz de integración contable a partir de las cuentas fijas del código
            // (CuentasContablesConst). Solo inserta filas cuyas dos cuentas existan en el plan de cuentas.
            EjecutarSql(migrationBuilder, @"
INSERT INTO MatrizIntegracionContable (ModuloOrigen, TipoOperacion, CuentaDebeId, CuentaHaberId, Descripcion, FechaCreacion, CreadoPor, Activo)
SELECT v.Modulo, v.Operacion, d.Id, h.Id, v.Descripcion, SYSUTCDATETIME(), 'System', 1
FROM (VALUES
 (N'Compras', N'FacturaCompraNeto', N'1.1.03.01', N'2.1.01.01', N'Factura de compra: ingreso de mercadería (neto) contra cuenta por pagar'),
 (N'Compras', N'FacturaCompraIva', N'1.1.04.01', N'2.1.01.01', N'Factura de compra: IVA crédito fiscal contra cuenta por pagar'),
 (N'Compras', N'PagoProveedorEfectivo', N'2.1.01.01', N'1.1.01.01', N'Pago a proveedor en efectivo (sale de Caja)'),
 (N'Compras', N'PagoProveedorBanco', N'2.1.01.01', N'1.1.01.02', N'Pago a proveedor por transferencia o tarjeta (sale de Banco)'),
 (N'Ventas', N'VentaContadoEfectivo', N'1.1.01.01', N'4.1.01.01', N'Venta al contado en efectivo: cobro contra ingreso (neto)'),
 (N'Ventas', N'VentaContadoBanco', N'1.1.01.02', N'4.1.01.01', N'Venta con tarjeta/transferencia: cobro contra ingreso (neto)'),
 (N'Ventas', N'VentaCredito', N'1.1.02.01', N'4.1.01.01', N'Venta a crédito: cuenta por cobrar contra ingreso (neto)'),
 (N'Ventas', N'IvaDebitoVenta', N'1.1.02.01', N'2.1.02.01', N'IVA débito de la venta (el Debe real es la cuenta de cobro de la venta: Caja, Banco o Clientes)'),
 (N'Ventas', N'CostoVenta', N'5.1.01.01', N'1.1.03.01', N'Costo de ventas (CPP) contra salida de inventario'),
 (N'Ventas', N'AbonoClienteEfectivo', N'1.1.01.01', N'1.1.02.01', N'Abono de cliente en efectivo'),
 (N'Ventas', N'AbonoClienteBanco', N'1.1.01.02', N'1.1.02.01', N'Abono de cliente por transferencia o tarjeta'),
 (N'Inventario', N'AjusteMerma', N'6.1.01.01', N'1.1.03.01', N'Ajuste de inventario negativo (merma/pérdida)'),
 (N'Inventario', N'AjusteSobrante', N'1.1.03.01', N'6.1.01.01', N'Ajuste de inventario positivo (sobrante)')
) AS v(Modulo, Operacion, CodDebe, CodHaber, Descripcion)
JOIN CuentasContables d ON d.Codigo = v.CodDebe
JOIN CuentasContables h ON h.Codigo = v.CodHaber
WHERE NOT EXISTS (SELECT 1 FROM MatrizIntegracionContable m WHERE m.ModuloOrigen = v.Modulo AND m.TipoOperacion = v.Operacion);");
        }

        /// <summary>En scripts idempotentes las tablas y columnas nuevas no existen al compilar el lote: el SQL se ejecuta con EXEC.</summary>
        private static void EjecutarSql(MigrationBuilder migrationBuilder, string sql) =>
            migrationBuilder.Sql("EXEC(N'" + sql.Replace("'", "''") + "')");

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AsientosContables_AsientosContables_AsientoRevertidoId",
                table: "AsientosContables");

            migrationBuilder.DropTable(
                name: "MatrizIntegracionContable");

            migrationBuilder.DropTable(
                name: "PagosProveedores");

            migrationBuilder.DropTable(
                name: "PeriodosContables");

            migrationBuilder.DropIndex(
                name: "IX_AsientosContables_AsientoRevertidoId",
                table: "AsientosContables");

            migrationBuilder.DropColumn(
                name: "AnuladoPor",
                table: "VentasClientes");

            migrationBuilder.DropColumn(
                name: "FechaAnulacion",
                table: "VentasClientes");

            migrationBuilder.DropColumn(
                name: "MotivoAnulacion",
                table: "VentasClientes");

            migrationBuilder.DropColumn(
                name: "PorcentajeComision",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "AnuladoPor",
                table: "OrdenesCompraProveedor");

            migrationBuilder.DropColumn(
                name: "AprobadoPor",
                table: "OrdenesCompraProveedor");

            migrationBuilder.DropColumn(
                name: "FechaAnulacion",
                table: "OrdenesCompraProveedor");

            migrationBuilder.DropColumn(
                name: "FechaAprobacion",
                table: "OrdenesCompraProveedor");

            migrationBuilder.DropColumn(
                name: "MotivoAnulacion",
                table: "OrdenesCompraProveedor");

            migrationBuilder.DropColumn(
                name: "AnuladoPor",
                table: "ComprasProveedores");

            migrationBuilder.DropColumn(
                name: "FechaAnulacion",
                table: "ComprasProveedores");

            migrationBuilder.DropColumn(
                name: "MotivoAnulacion",
                table: "ComprasProveedores");

            migrationBuilder.DropColumn(
                name: "SaldoPendiente",
                table: "ComprasProveedores");

            migrationBuilder.DropColumn(
                name: "TotalPagado",
                table: "ComprasProveedores");

            migrationBuilder.DropColumn(
                name: "AsientoRevertidoId",
                table: "AsientosContables");
        }
    }
}
