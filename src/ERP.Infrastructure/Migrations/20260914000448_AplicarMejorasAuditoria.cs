using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AplicarMejorasAuditoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AjustesInventario_Productos_ProductoId",
                table: "AjustesInventario");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesFacturaCompra_FacturasCompra_FacturaCompraId",
                table: "DetallesFacturaCompra");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesFacturaCompra_Productos_ProductoId",
                table: "DetallesFacturaCompra");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesOrdenCompra_OrdenesCompra_OrdenCompraId",
                table: "DetallesOrdenCompra");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesOrdenCompra_Productos_ProductoId",
                table: "DetallesOrdenCompra");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesRecepcionCompra_Productos_ProductoId",
                table: "DetallesRecepcionCompra");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesRecepcionCompra_RecepcionesCompra_RecepcionCompraId",
                table: "DetallesRecepcionCompra");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesVenta_Productos_ProductoId",
                table: "DetallesVenta");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesVenta_Ventas_VentaId",
                table: "DetallesVenta");

            migrationBuilder.DropForeignKey(
                name: "FK_FacturasCompra_OrdenesCompra_OrdenCompraId",
                table: "FacturasCompra");

            migrationBuilder.DropForeignKey(
                name: "FK_FacturasCompra_Proveedores_ProveedorId",
                table: "FacturasCompra");

            migrationBuilder.DropForeignKey(
                name: "FK_FacturasCompra_RecepcionesCompra_RecepcionCompraId",
                table: "FacturasCompra");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosKardex_Productos_ProductoId",
                table: "MovimientosKardex");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdenesCompra_Proveedores_ProveedorId",
                table: "OrdenesCompra");

            migrationBuilder.DropForeignKey(
                name: "FK_PagosVenta_Ventas_VentaId",
                table: "PagosVenta");

            migrationBuilder.DropForeignKey(
                name: "FK_RecepcionesCompra_OrdenesCompra_OrdenCompraId",
                table: "RecepcionesCompra");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Clientes_ClienteId",
                table: "Ventas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Ventas",
                table: "Ventas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RecepcionesCompra",
                table: "RecepcionesCompra");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PagosVenta",
                table: "PagosVenta");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrdenesCompra",
                table: "OrdenesCompra");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MovimientosKardex",
                table: "MovimientosKardex");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FacturasCompra",
                table: "FacturasCompra");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DetallesFacturaCompra",
                table: "DetallesFacturaCompra");

            migrationBuilder.RenameTable(
                name: "Ventas",
                newName: "VentasClientes");

            migrationBuilder.RenameTable(
                name: "RecepcionesCompra",
                newName: "RecepcionesMercaderia");

            migrationBuilder.RenameTable(
                name: "PagosVenta",
                newName: "CobrosClientes");

            migrationBuilder.RenameTable(
                name: "OrdenesCompra",
                newName: "OrdenesCompraProveedor");

            migrationBuilder.RenameTable(
                name: "MovimientosKardex",
                newName: "KardexInventario");

            migrationBuilder.RenameTable(
                name: "FacturasCompra",
                newName: "ComprasProveedores");

            migrationBuilder.RenameTable(
                name: "DetallesFacturaCompra",
                newName: "LineasCompraProveedor");

            migrationBuilder.RenameIndex(
                name: "IX_Ventas_ClienteId",
                table: "VentasClientes",
                newName: "IX_VentasClientes_ClienteId");

            migrationBuilder.RenameIndex(
                name: "IX_RecepcionesCompra_OrdenCompraId",
                table: "RecepcionesMercaderia",
                newName: "IX_RecepcionesMercaderia_OrdenCompraId");

            migrationBuilder.RenameIndex(
                name: "IX_PagosVenta_VentaId",
                table: "CobrosClientes",
                newName: "IX_CobrosClientes_VentaId");

            migrationBuilder.RenameIndex(
                name: "IX_OrdenesCompra_ProveedorId",
                table: "OrdenesCompraProveedor",
                newName: "IX_OrdenesCompraProveedor_ProveedorId");

            migrationBuilder.RenameIndex(
                name: "IX_MovimientosKardex_ProductoId",
                table: "KardexInventario",
                newName: "IX_KardexInventario_ProductoId");

            migrationBuilder.RenameIndex(
                name: "IX_FacturasCompra_RecepcionCompraId",
                table: "ComprasProveedores",
                newName: "IX_ComprasProveedores_RecepcionCompraId");

            migrationBuilder.RenameIndex(
                name: "IX_FacturasCompra_ProveedorId",
                table: "ComprasProveedores",
                newName: "IX_ComprasProveedores_ProveedorId");

            migrationBuilder.RenameIndex(
                name: "IX_FacturasCompra_OrdenCompraId",
                table: "ComprasProveedores",
                newName: "IX_ComprasProveedores_OrdenCompraId");

            migrationBuilder.RenameIndex(
                name: "IX_DetallesFacturaCompra_ProductoId",
                table: "LineasCompraProveedor",
                newName: "IX_LineasCompraProveedor_ProductoId");

            migrationBuilder.RenameIndex(
                name: "IX_DetallesFacturaCompra_FacturaCompraId",
                table: "LineasCompraProveedor",
                newName: "IX_LineasCompraProveedor_FacturaCompraId");

            migrationBuilder.AddColumn<int>(
                name: "DetalleOrdenCompraId",
                table: "DetallesRecepcionCompra",
                type: "integer",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_VentasClientes",
                table: "VentasClientes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RecepcionesMercaderia",
                table: "RecepcionesMercaderia",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CobrosClientes",
                table: "CobrosClientes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrdenesCompraProveedor",
                table: "OrdenesCompraProveedor",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KardexInventario",
                table: "KardexInventario",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ComprasProveedores",
                table: "ComprasProveedores",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LineasCompraProveedor",
                table: "LineasCompraProveedor",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesRecepcionCompra_DetalleOrdenCompraId",
                table: "DetallesRecepcionCompra",
                column: "DetalleOrdenCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_AjustesInventario_AsientoContableId",
                table: "AjustesInventario",
                column: "AsientoContableId");

            migrationBuilder.CreateIndex(
                name: "IX_VentasClientes_AsientoCostoVentaId",
                table: "VentasClientes",
                column: "AsientoCostoVentaId");

            migrationBuilder.CreateIndex(
                name: "IX_VentasClientes_AsientoVentaId",
                table: "VentasClientes",
                column: "AsientoVentaId");

            migrationBuilder.CreateIndex(
                name: "IX_CobrosClientes_AsientoPagoId",
                table: "CobrosClientes",
                column: "AsientoPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_ComprasProveedores_AsientoContableId",
                table: "ComprasProveedores",
                column: "AsientoContableId");

            migrationBuilder.AddForeignKey(
                name: "FK_AjustesInventario_AsientosContables_AsientoContableId",
                table: "AjustesInventario",
                column: "AsientoContableId",
                principalTable: "AsientosContables",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AjustesInventario_Productos_ProductoId",
                table: "AjustesInventario",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CobrosClientes_AsientosContables_AsientoPagoId",
                table: "CobrosClientes",
                column: "AsientoPagoId",
                principalTable: "AsientosContables",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CobrosClientes_VentasClientes_VentaId",
                table: "CobrosClientes",
                column: "VentaId",
                principalTable: "VentasClientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ComprasProveedores_AsientosContables_AsientoContableId",
                table: "ComprasProveedores",
                column: "AsientoContableId",
                principalTable: "AsientosContables",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ComprasProveedores_OrdenesCompraProveedor_OrdenCompraId",
                table: "ComprasProveedores",
                column: "OrdenCompraId",
                principalTable: "OrdenesCompraProveedor",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ComprasProveedores_Proveedores_ProveedorId",
                table: "ComprasProveedores",
                column: "ProveedorId",
                principalTable: "Proveedores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ComprasProveedores_RecepcionesMercaderia_RecepcionCompraId",
                table: "ComprasProveedores",
                column: "RecepcionCompraId",
                principalTable: "RecepcionesMercaderia",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesOrdenCompra_OrdenesCompraProveedor_OrdenCompraId",
                table: "DetallesOrdenCompra",
                column: "OrdenCompraId",
                principalTable: "OrdenesCompraProveedor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesOrdenCompra_Productos_ProductoId",
                table: "DetallesOrdenCompra",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesRecepcionCompra_DetallesOrdenCompra_DetalleOrdenCom~",
                table: "DetallesRecepcionCompra",
                column: "DetalleOrdenCompraId",
                principalTable: "DetallesOrdenCompra",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesRecepcionCompra_Productos_ProductoId",
                table: "DetallesRecepcionCompra",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesRecepcionCompra_RecepcionesMercaderia_RecepcionComp~",
                table: "DetallesRecepcionCompra",
                column: "RecepcionCompraId",
                principalTable: "RecepcionesMercaderia",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesVenta_Productos_ProductoId",
                table: "DetallesVenta",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesVenta_VentasClientes_VentaId",
                table: "DetallesVenta",
                column: "VentaId",
                principalTable: "VentasClientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_KardexInventario_Productos_ProductoId",
                table: "KardexInventario",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LineasCompraProveedor_ComprasProveedores_FacturaCompraId",
                table: "LineasCompraProveedor",
                column: "FacturaCompraId",
                principalTable: "ComprasProveedores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LineasCompraProveedor_Productos_ProductoId",
                table: "LineasCompraProveedor",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdenesCompraProveedor_Proveedores_ProveedorId",
                table: "OrdenesCompraProveedor",
                column: "ProveedorId",
                principalTable: "Proveedores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecepcionesMercaderia_OrdenesCompraProveedor_OrdenCompraId",
                table: "RecepcionesMercaderia",
                column: "OrdenCompraId",
                principalTable: "OrdenesCompraProveedor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VentasClientes_AsientosContables_AsientoCostoVentaId",
                table: "VentasClientes",
                column: "AsientoCostoVentaId",
                principalTable: "AsientosContables",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_VentasClientes_AsientosContables_AsientoVentaId",
                table: "VentasClientes",
                column: "AsientoVentaId",
                principalTable: "AsientosContables",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_VentasClientes_Clientes_ClienteId",
                table: "VentasClientes",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AjustesInventario_AsientosContables_AsientoContableId",
                table: "AjustesInventario");

            migrationBuilder.DropForeignKey(
                name: "FK_AjustesInventario_Productos_ProductoId",
                table: "AjustesInventario");

            migrationBuilder.DropForeignKey(
                name: "FK_CobrosClientes_AsientosContables_AsientoPagoId",
                table: "CobrosClientes");

            migrationBuilder.DropForeignKey(
                name: "FK_CobrosClientes_VentasClientes_VentaId",
                table: "CobrosClientes");

            migrationBuilder.DropForeignKey(
                name: "FK_ComprasProveedores_AsientosContables_AsientoContableId",
                table: "ComprasProveedores");

            migrationBuilder.DropForeignKey(
                name: "FK_ComprasProveedores_OrdenesCompraProveedor_OrdenCompraId",
                table: "ComprasProveedores");

            migrationBuilder.DropForeignKey(
                name: "FK_ComprasProveedores_Proveedores_ProveedorId",
                table: "ComprasProveedores");

            migrationBuilder.DropForeignKey(
                name: "FK_ComprasProveedores_RecepcionesMercaderia_RecepcionCompraId",
                table: "ComprasProveedores");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesOrdenCompra_OrdenesCompraProveedor_OrdenCompraId",
                table: "DetallesOrdenCompra");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesOrdenCompra_Productos_ProductoId",
                table: "DetallesOrdenCompra");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesRecepcionCompra_DetallesOrdenCompra_DetalleOrdenCom~",
                table: "DetallesRecepcionCompra");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesRecepcionCompra_Productos_ProductoId",
                table: "DetallesRecepcionCompra");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesRecepcionCompra_RecepcionesMercaderia_RecepcionComp~",
                table: "DetallesRecepcionCompra");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesVenta_Productos_ProductoId",
                table: "DetallesVenta");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesVenta_VentasClientes_VentaId",
                table: "DetallesVenta");

            migrationBuilder.DropForeignKey(
                name: "FK_KardexInventario_Productos_ProductoId",
                table: "KardexInventario");

            migrationBuilder.DropForeignKey(
                name: "FK_LineasCompraProveedor_ComprasProveedores_FacturaCompraId",
                table: "LineasCompraProveedor");

            migrationBuilder.DropForeignKey(
                name: "FK_LineasCompraProveedor_Productos_ProductoId",
                table: "LineasCompraProveedor");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdenesCompraProveedor_Proveedores_ProveedorId",
                table: "OrdenesCompraProveedor");

            migrationBuilder.DropForeignKey(
                name: "FK_RecepcionesMercaderia_OrdenesCompraProveedor_OrdenCompraId",
                table: "RecepcionesMercaderia");

            migrationBuilder.DropForeignKey(
                name: "FK_VentasClientes_AsientosContables_AsientoCostoVentaId",
                table: "VentasClientes");

            migrationBuilder.DropForeignKey(
                name: "FK_VentasClientes_AsientosContables_AsientoVentaId",
                table: "VentasClientes");

            migrationBuilder.DropForeignKey(
                name: "FK_VentasClientes_Clientes_ClienteId",
                table: "VentasClientes");

            migrationBuilder.DropIndex(
                name: "IX_DetallesRecepcionCompra_DetalleOrdenCompraId",
                table: "DetallesRecepcionCompra");

            migrationBuilder.DropIndex(
                name: "IX_AjustesInventario_AsientoContableId",
                table: "AjustesInventario");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VentasClientes",
                table: "VentasClientes");

            migrationBuilder.DropIndex(
                name: "IX_VentasClientes_AsientoCostoVentaId",
                table: "VentasClientes");

            migrationBuilder.DropIndex(
                name: "IX_VentasClientes_AsientoVentaId",
                table: "VentasClientes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RecepcionesMercaderia",
                table: "RecepcionesMercaderia");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrdenesCompraProveedor",
                table: "OrdenesCompraProveedor");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LineasCompraProveedor",
                table: "LineasCompraProveedor");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KardexInventario",
                table: "KardexInventario");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ComprasProveedores",
                table: "ComprasProveedores");

            migrationBuilder.DropIndex(
                name: "IX_ComprasProveedores_AsientoContableId",
                table: "ComprasProveedores");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CobrosClientes",
                table: "CobrosClientes");

            migrationBuilder.DropIndex(
                name: "IX_CobrosClientes_AsientoPagoId",
                table: "CobrosClientes");

            migrationBuilder.DropColumn(
                name: "DetalleOrdenCompraId",
                table: "DetallesRecepcionCompra");

            migrationBuilder.RenameTable(
                name: "VentasClientes",
                newName: "Ventas");

            migrationBuilder.RenameTable(
                name: "RecepcionesMercaderia",
                newName: "RecepcionesCompra");

            migrationBuilder.RenameTable(
                name: "OrdenesCompraProveedor",
                newName: "OrdenesCompra");

            migrationBuilder.RenameTable(
                name: "LineasCompraProveedor",
                newName: "DetallesFacturaCompra");

            migrationBuilder.RenameTable(
                name: "KardexInventario",
                newName: "MovimientosKardex");

            migrationBuilder.RenameTable(
                name: "ComprasProveedores",
                newName: "FacturasCompra");

            migrationBuilder.RenameTable(
                name: "CobrosClientes",
                newName: "PagosVenta");

            migrationBuilder.RenameIndex(
                name: "IX_VentasClientes_ClienteId",
                table: "Ventas",
                newName: "IX_Ventas_ClienteId");

            migrationBuilder.RenameIndex(
                name: "IX_RecepcionesMercaderia_OrdenCompraId",
                table: "RecepcionesCompra",
                newName: "IX_RecepcionesCompra_OrdenCompraId");

            migrationBuilder.RenameIndex(
                name: "IX_OrdenesCompraProveedor_ProveedorId",
                table: "OrdenesCompra",
                newName: "IX_OrdenesCompra_ProveedorId");

            migrationBuilder.RenameIndex(
                name: "IX_LineasCompraProveedor_ProductoId",
                table: "DetallesFacturaCompra",
                newName: "IX_DetallesFacturaCompra_ProductoId");

            migrationBuilder.RenameIndex(
                name: "IX_LineasCompraProveedor_FacturaCompraId",
                table: "DetallesFacturaCompra",
                newName: "IX_DetallesFacturaCompra_FacturaCompraId");

            migrationBuilder.RenameIndex(
                name: "IX_KardexInventario_ProductoId",
                table: "MovimientosKardex",
                newName: "IX_MovimientosKardex_ProductoId");

            migrationBuilder.RenameIndex(
                name: "IX_ComprasProveedores_RecepcionCompraId",
                table: "FacturasCompra",
                newName: "IX_FacturasCompra_RecepcionCompraId");

            migrationBuilder.RenameIndex(
                name: "IX_ComprasProveedores_ProveedorId",
                table: "FacturasCompra",
                newName: "IX_FacturasCompra_ProveedorId");

            migrationBuilder.RenameIndex(
                name: "IX_ComprasProveedores_OrdenCompraId",
                table: "FacturasCompra",
                newName: "IX_FacturasCompra_OrdenCompraId");

            migrationBuilder.RenameIndex(
                name: "IX_CobrosClientes_VentaId",
                table: "PagosVenta",
                newName: "IX_PagosVenta_VentaId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Ventas",
                table: "Ventas",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RecepcionesCompra",
                table: "RecepcionesCompra",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrdenesCompra",
                table: "OrdenesCompra",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DetallesFacturaCompra",
                table: "DetallesFacturaCompra",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MovimientosKardex",
                table: "MovimientosKardex",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FacturasCompra",
                table: "FacturasCompra",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PagosVenta",
                table: "PagosVenta",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AjustesInventario_Productos_ProductoId",
                table: "AjustesInventario",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesFacturaCompra_FacturasCompra_FacturaCompraId",
                table: "DetallesFacturaCompra",
                column: "FacturaCompraId",
                principalTable: "FacturasCompra",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesFacturaCompra_Productos_ProductoId",
                table: "DetallesFacturaCompra",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesOrdenCompra_OrdenesCompra_OrdenCompraId",
                table: "DetallesOrdenCompra",
                column: "OrdenCompraId",
                principalTable: "OrdenesCompra",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesOrdenCompra_Productos_ProductoId",
                table: "DetallesOrdenCompra",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesRecepcionCompra_Productos_ProductoId",
                table: "DetallesRecepcionCompra",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesRecepcionCompra_RecepcionesCompra_RecepcionCompraId",
                table: "DetallesRecepcionCompra",
                column: "RecepcionCompraId",
                principalTable: "RecepcionesCompra",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesVenta_Productos_ProductoId",
                table: "DetallesVenta",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesVenta_Ventas_VentaId",
                table: "DetallesVenta",
                column: "VentaId",
                principalTable: "Ventas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FacturasCompra_OrdenesCompra_OrdenCompraId",
                table: "FacturasCompra",
                column: "OrdenCompraId",
                principalTable: "OrdenesCompra",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FacturasCompra_Proveedores_ProveedorId",
                table: "FacturasCompra",
                column: "ProveedorId",
                principalTable: "Proveedores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FacturasCompra_RecepcionesCompra_RecepcionCompraId",
                table: "FacturasCompra",
                column: "RecepcionCompraId",
                principalTable: "RecepcionesCompra",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosKardex_Productos_ProductoId",
                table: "MovimientosKardex",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdenesCompra_Proveedores_ProveedorId",
                table: "OrdenesCompra",
                column: "ProveedorId",
                principalTable: "Proveedores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PagosVenta_Ventas_VentaId",
                table: "PagosVenta",
                column: "VentaId",
                principalTable: "Ventas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecepcionesCompra_OrdenesCompra_OrdenCompraId",
                table: "RecepcionesCompra",
                column: "OrdenCompraId",
                principalTable: "OrdenesCompra",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Clientes_ClienteId",
                table: "Ventas",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
