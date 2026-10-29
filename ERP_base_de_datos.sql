IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE TABLE [AsientosContables] (
        [Id] int NOT NULL IDENTITY,
        [NumeroAsiento] nvarchar(max) NOT NULL,
        [Fecha] datetime2 NOT NULL,
        [Glosa] nvarchar(max) NOT NULL,
        [ModuloOrigen] nvarchar(max) NOT NULL,
        [DocumentoOrigenTipo] nvarchar(max) NULL,
        [DocumentoOrigenId] int NULL,
        [TotalDebe] decimal(18,4) NOT NULL,
        [TotalHaber] decimal(18,4) NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [CreadoPor] nvarchar(max) NULL,
        [ModificadoPor] nvarchar(max) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_AsientosContables] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE TABLE [Auditorias] (
        [Id] int NOT NULL IDENTITY,
        [Modulo] nvarchar(max) NOT NULL,
        [Accion] nvarchar(max) NOT NULL,
        [Entidad] nvarchar(max) NOT NULL,
        [EntidadId] nvarchar(max) NULL,
        [UsuarioEmail] nvarchar(max) NOT NULL,
        [Detalles] nvarchar(max) NULL,
        [IpAddress] nvarchar(max) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [CreadoPor] nvarchar(max) NULL,
        [ModificadoPor] nvarchar(max) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Auditorias] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE TABLE [Categorias] (
        [Id] int NOT NULL IDENTITY,
        [Codigo] nvarchar(max) NOT NULL,
        [Nombre] nvarchar(max) NOT NULL,
        [Descripcion] nvarchar(max) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [CreadoPor] nvarchar(max) NULL,
        [ModificadoPor] nvarchar(max) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Categorias] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE TABLE [Clientes] (
        [Id] int NOT NULL IDENTITY,
        [Rut] nvarchar(450) NOT NULL,
        [RazonSocial] nvarchar(max) NOT NULL,
        [Giro] nvarchar(max) NULL,
        [Contacto] nvarchar(max) NULL,
        [Direccion] nvarchar(max) NULL,
        [Telefono] nvarchar(max) NULL,
        [Email] nvarchar(max) NOT NULL,
        [LimiteCredito] decimal(18,4) NOT NULL,
        [SaldoPendiente] decimal(18,4) NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [CreadoPor] nvarchar(max) NULL,
        [ModificadoPor] nvarchar(max) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Clientes] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE TABLE [CuentasContables] (
        [Id] int NOT NULL IDENTITY,
        [Codigo] nvarchar(450) NOT NULL,
        [Nombre] nvarchar(max) NOT NULL,
        [Tipo] int NOT NULL,
        [Naturaleza] int NOT NULL,
        [Nivel] int NOT NULL,
        [CuentaPadreId] int NULL,
        [PermiteMovimiento] bit NOT NULL,
        [SaldoActual] decimal(18,4) NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [CreadoPor] nvarchar(max) NULL,
        [ModificadoPor] nvarchar(max) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_CuentasContables] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CuentasContables_CuentasContables_CuentaPadreId] FOREIGN KEY ([CuentaPadreId]) REFERENCES [CuentasContables] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE TABLE [Permisos] (
        [Id] int NOT NULL IDENTITY,
        [Codigo] nvarchar(max) NOT NULL,
        [Modulo] nvarchar(max) NOT NULL,
        [Descripcion] nvarchar(max) NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [CreadoPor] nvarchar(max) NULL,
        [ModificadoPor] nvarchar(max) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Permisos] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE TABLE [Proveedores] (
        [Id] int NOT NULL IDENTITY,
        [Rut] nvarchar(450) NOT NULL,
        [RazonSocial] nvarchar(max) NOT NULL,
        [Giro] nvarchar(max) NULL,
        [Contacto] nvarchar(max) NULL,
        [Direccion] nvarchar(max) NULL,
        [Telefono] nvarchar(max) NULL,
        [Email] nvarchar(max) NOT NULL,
        [CondicionesPago] nvarchar(max) NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [CreadoPor] nvarchar(max) NULL,
        [ModificadoPor] nvarchar(max) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Proveedores] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE TABLE [Roles] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(max) NOT NULL,
        [Descripcion] nvarchar(max) NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [CreadoPor] nvarchar(max) NULL,
        [ModificadoPor] nvarchar(max) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE TABLE [VentasClientes] (
        [Id] int NOT NULL IDENTITY,
        [NumeroFactura] nvarchar(max) NOT NULL,
        [FechaEmision] datetime2 NOT NULL,
        [FechaVencimiento] datetime2 NULL,
        [ClienteId] int NOT NULL,
        [Estado] int NOT NULL,
        [Subtotal] decimal(18,4) NOT NULL,
        [ImpuestoIva] decimal(18,4) NOT NULL,
        [Descuento] decimal(18,4) NOT NULL,
        [Total] decimal(18,4) NOT NULL,
        [TotalPagado] decimal(18,4) NOT NULL,
        [SaldoPendiente] decimal(18,4) NOT NULL,
        [CostoVentaTotal] decimal(18,4) NOT NULL,
        [MetodoPago] nvarchar(max) NOT NULL,
        [AsientoVentaId] int NULL,
        [AsientoCostoVentaId] int NULL,
        [Observaciones] nvarchar(max) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [CreadoPor] nvarchar(max) NULL,
        [ModificadoPor] nvarchar(max) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_VentasClientes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_VentasClientes_AsientosContables_AsientoCostoVentaId] FOREIGN KEY ([AsientoCostoVentaId]) REFERENCES [AsientosContables] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_VentasClientes_AsientosContables_AsientoVentaId] FOREIGN KEY ([AsientoVentaId]) REFERENCES [AsientosContables] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_VentasClientes_Clientes_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clientes] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE TABLE [DetallesAsientoContable] (
        [Id] int NOT NULL IDENTITY,
        [AsientoContableId] int NOT NULL,
        [CuentaContableId] int NOT NULL,
        [Debe] decimal(18,4) NOT NULL,
        [Haber] decimal(18,4) NOT NULL,
        [GlosaLinea] nvarchar(max) NULL,
        [Referencia] nvarchar(max) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [CreadoPor] nvarchar(max) NULL,
        [ModificadoPor] nvarchar(max) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_DetallesAsientoContable] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DetallesAsientoContable_AsientosContables_AsientoContableId] FOREIGN KEY ([AsientoContableId]) REFERENCES [AsientosContables] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_DetallesAsientoContable_CuentasContables_CuentaContableId] FOREIGN KEY ([CuentaContableId]) REFERENCES [CuentasContables] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE TABLE [Productos] (
        [Id] int NOT NULL IDENTITY,
        [Sku] nvarchar(450) NOT NULL,
        [Nombre] nvarchar(max) NOT NULL,
        [Descripcion] nvarchar(max) NULL,
        [CategoriaId] int NOT NULL,
        [UnidadMedida] nvarchar(max) NOT NULL,
        [PrecioVenta] decimal(18,4) NOT NULL,
        [CostoBase] decimal(18,4) NOT NULL,
        [CostoPromedioPonderado] decimal(18,4) NOT NULL,
        [StockActual] decimal(18,4) NOT NULL,
        [StockMinimo] decimal(18,4) NOT NULL,
        [PuntoReorden] decimal(18,4) NOT NULL,
        [CuentaInventarioId] int NULL,
        [CuentaVentaId] int NULL,
        [CuentaCostoVentaId] int NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [CreadoPor] nvarchar(max) NULL,
        [ModificadoPor] nvarchar(max) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Productos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Productos_Categorias_CategoriaId] FOREIGN KEY ([CategoriaId]) REFERENCES [Categorias] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Productos_CuentasContables_CuentaCostoVentaId] FOREIGN KEY ([CuentaCostoVentaId]) REFERENCES [CuentasContables] ([Id]),
        CONSTRAINT [FK_Productos_CuentasContables_CuentaInventarioId] FOREIGN KEY ([CuentaInventarioId]) REFERENCES [CuentasContables] ([Id]),
        CONSTRAINT [FK_Productos_CuentasContables_CuentaVentaId] FOREIGN KEY ([CuentaVentaId]) REFERENCES [CuentasContables] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE TABLE [OrdenesCompraProveedor] (
        [Id] int NOT NULL IDENTITY,
        [NumeroOrden] nvarchar(max) NOT NULL,
        [FechaEmision] datetime2 NOT NULL,
        [FechaEntregaEsperada] datetime2 NULL,
        [ProveedorId] int NOT NULL,
        [Estado] int NOT NULL,
        [Subtotal] decimal(18,4) NOT NULL,
        [ImpuestoIva] decimal(18,4) NOT NULL,
        [Total] decimal(18,4) NOT NULL,
        [Observaciones] nvarchar(max) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [CreadoPor] nvarchar(max) NULL,
        [ModificadoPor] nvarchar(max) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_OrdenesCompraProveedor] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrdenesCompraProveedor_Proveedores_ProveedorId] FOREIGN KEY ([ProveedorId]) REFERENCES [Proveedores] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE TABLE [RolPermisos] (
        [RolId] int NOT NULL,
        [PermisoId] int NOT NULL,
        CONSTRAINT [PK_RolPermisos] PRIMARY KEY ([RolId], [PermisoId]),
        CONSTRAINT [FK_RolPermisos_Permisos_PermisoId] FOREIGN KEY ([PermisoId]) REFERENCES [Permisos] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_RolPermisos_Roles_RolId] FOREIGN KEY ([RolId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE TABLE [Usuarios] (
        [Id] int NOT NULL IDENTITY,
        [NombreCompleto] nvarchar(max) NOT NULL,
        [Email] nvarchar(450) NOT NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [Rut] nvarchar(max) NOT NULL,
        [RolId] int NOT NULL,
        [UltimoAcceso] datetime2 NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [CreadoPor] nvarchar(max) NULL,
        [ModificadoPor] nvarchar(max) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Usuarios_Roles_RolId] FOREIGN KEY ([RolId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE TABLE [CobrosClientes] (
        [Id] int NOT NULL IDENTITY,
        [VentaId] int NOT NULL,
        [FechaPago] datetime2 NOT NULL,
        [Monto] decimal(18,4) NOT NULL,
        [MetodoPago] nvarchar(max) NOT NULL,
        [ReferenciaComprobante] nvarchar(max) NULL,
        [AsientoPagoId] int NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [CreadoPor] nvarchar(max) NULL,
        [ModificadoPor] nvarchar(max) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_CobrosClientes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CobrosClientes_AsientosContables_AsientoPagoId] FOREIGN KEY ([AsientoPagoId]) REFERENCES [AsientosContables] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CobrosClientes_VentasClientes_VentaId] FOREIGN KEY ([VentaId]) REFERENCES [VentasClientes] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE TABLE [AjustesInventario] (
        [Id] int NOT NULL IDENTITY,
        [NumeroAjuste] nvarchar(max) NOT NULL,
        [Fecha] datetime2 NOT NULL,
        [ProductoId] int NOT NULL,
        [CantidadAnterior] decimal(18,4) NOT NULL,
        [CantidadAjuste] decimal(18,4) NOT NULL,
        [CantidadNueva] decimal(18,4) NOT NULL,
        [CostoUnitario] decimal(18,4) NOT NULL,
        [Motivo] nvarchar(max) NOT NULL,
        [AsientoContableId] int NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [CreadoPor] nvarchar(max) NULL,
        [ModificadoPor] nvarchar(max) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_AjustesInventario] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AjustesInventario_AsientosContables_AsientoContableId] FOREIGN KEY ([AsientoContableId]) REFERENCES [AsientosContables] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AjustesInventario_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE TABLE [DetallesVenta] (
        [Id] int NOT NULL IDENTITY,
        [VentaId] int NOT NULL,
        [ProductoId] int NOT NULL,
        [Cantidad] decimal(18,4) NOT NULL,
        [PrecioUnitario] decimal(18,4) NOT NULL,
        [CostoUnitarioSnapshot] decimal(18,4) NOT NULL,
        [Descuento] decimal(18,4) NOT NULL,
        [Subtotal] decimal(18,4) NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [CreadoPor] nvarchar(max) NULL,
        [ModificadoPor] nvarchar(max) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_DetallesVenta] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DetallesVenta_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DetallesVenta_VentasClientes_VentaId] FOREIGN KEY ([VentaId]) REFERENCES [VentasClientes] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE TABLE [KardexInventario] (
        [Id] int NOT NULL IDENTITY,
        [ProductoId] int NOT NULL,
        [Fecha] datetime2 NOT NULL,
        [TipoMovimiento] int NOT NULL,
        [DocumentoTipo] nvarchar(max) NOT NULL,
        [DocumentoNumero] nvarchar(max) NOT NULL,
        [DocumentoId] int NULL,
        [CantidadEntrada] decimal(18,4) NOT NULL,
        [CostoUnitarioEntrada] decimal(18,4) NOT NULL,
        [CantidadSalida] decimal(18,4) NOT NULL,
        [CostoUnitarioSalida] decimal(18,4) NOT NULL,
        [SaldoCantidad] decimal(18,4) NOT NULL,
        [CostoPromedioPonderadoResultante] decimal(18,4) NOT NULL,
        [SaldoValorizado] decimal(18,4) NOT NULL,
        [Glosa] nvarchar(max) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [CreadoPor] nvarchar(max) NULL,
        [ModificadoPor] nvarchar(max) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_KardexInventario] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_KardexInventario_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE TABLE [DetallesOrdenCompra] (
        [Id] int NOT NULL IDENTITY,
        [OrdenCompraId] int NOT NULL,
        [ProductoId] int NOT NULL,
        [Cantidad] decimal(18,4) NOT NULL,
        [CantidadRecibida] decimal(18,4) NOT NULL,
        [PrecioUnitario] decimal(18,4) NOT NULL,
        [Subtotal] decimal(18,4) NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [CreadoPor] nvarchar(max) NULL,
        [ModificadoPor] nvarchar(max) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_DetallesOrdenCompra] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DetallesOrdenCompra_OrdenesCompraProveedor_OrdenCompraId] FOREIGN KEY ([OrdenCompraId]) REFERENCES [OrdenesCompraProveedor] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_DetallesOrdenCompra_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE TABLE [RecepcionesMercaderia] (
        [Id] int NOT NULL IDENTITY,
        [NumeroRecepcion] nvarchar(max) NOT NULL,
        [FechaRecepcion] datetime2 NOT NULL,
        [OrdenCompraId] int NOT NULL,
        [GuiaDespachoProveedor] nvarchar(max) NULL,
        [RecibidoPor] nvarchar(max) NULL,
        [Observaciones] nvarchar(max) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [CreadoPor] nvarchar(max) NULL,
        [ModificadoPor] nvarchar(max) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_RecepcionesMercaderia] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RecepcionesMercaderia_OrdenesCompraProveedor_OrdenCompraId] FOREIGN KEY ([OrdenCompraId]) REFERENCES [OrdenesCompraProveedor] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE TABLE [ComprasProveedores] (
        [Id] int NOT NULL IDENTITY,
        [NumeroFactura] nvarchar(max) NOT NULL,
        [FechaEmision] datetime2 NOT NULL,
        [FechaVencimiento] datetime2 NOT NULL,
        [ProveedorId] int NOT NULL,
        [OrdenCompraId] int NULL,
        [RecepcionCompraId] int NULL,
        [Estado] int NOT NULL,
        [Subtotal] decimal(18,4) NOT NULL,
        [ImpuestoIva] decimal(18,4) NOT NULL,
        [Total] decimal(18,4) NOT NULL,
        [AsientoContableId] int NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [CreadoPor] nvarchar(max) NULL,
        [ModificadoPor] nvarchar(max) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_ComprasProveedores] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ComprasProveedores_AsientosContables_AsientoContableId] FOREIGN KEY ([AsientoContableId]) REFERENCES [AsientosContables] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ComprasProveedores_OrdenesCompraProveedor_OrdenCompraId] FOREIGN KEY ([OrdenCompraId]) REFERENCES [OrdenesCompraProveedor] ([Id]),
        CONSTRAINT [FK_ComprasProveedores_Proveedores_ProveedorId] FOREIGN KEY ([ProveedorId]) REFERENCES [Proveedores] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ComprasProveedores_RecepcionesMercaderia_RecepcionCompraId] FOREIGN KEY ([RecepcionCompraId]) REFERENCES [RecepcionesMercaderia] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE TABLE [DetallesRecepcionCompra] (
        [Id] int NOT NULL IDENTITY,
        [RecepcionCompraId] int NOT NULL,
        [ProductoId] int NOT NULL,
        [DetalleOrdenCompraId] int NULL,
        [CantidadRecibida] decimal(18,4) NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [CreadoPor] nvarchar(max) NULL,
        [ModificadoPor] nvarchar(max) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_DetallesRecepcionCompra] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DetallesRecepcionCompra_DetallesOrdenCompra_DetalleOrdenCompraId] FOREIGN KEY ([DetalleOrdenCompraId]) REFERENCES [DetallesOrdenCompra] ([Id]),
        CONSTRAINT [FK_DetallesRecepcionCompra_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DetallesRecepcionCompra_RecepcionesMercaderia_RecepcionCompraId] FOREIGN KEY ([RecepcionCompraId]) REFERENCES [RecepcionesMercaderia] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE TABLE [LineasCompraProveedor] (
        [Id] int NOT NULL IDENTITY,
        [FacturaCompraId] int NOT NULL,
        [ProductoId] int NOT NULL,
        [Cantidad] decimal(18,4) NOT NULL,
        [PrecioUnitario] decimal(18,4) NOT NULL,
        [Subtotal] decimal(18,4) NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [CreadoPor] nvarchar(max) NULL,
        [ModificadoPor] nvarchar(max) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_LineasCompraProveedor] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LineasCompraProveedor_ComprasProveedores_FacturaCompraId] FOREIGN KEY ([FacturaCompraId]) REFERENCES [ComprasProveedores] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_LineasCompraProveedor_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AjustesInventario_AsientoContableId] ON [AjustesInventario] ([AsientoContableId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AjustesInventario_ProductoId] ON [AjustesInventario] ([ProductoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Clientes_Rut] ON [Clientes] ([Rut]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CobrosClientes_AsientoPagoId] ON [CobrosClientes] ([AsientoPagoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CobrosClientes_VentaId] ON [CobrosClientes] ([VentaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ComprasProveedores_AsientoContableId] ON [ComprasProveedores] ([AsientoContableId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ComprasProveedores_OrdenCompraId] ON [ComprasProveedores] ([OrdenCompraId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ComprasProveedores_ProveedorId] ON [ComprasProveedores] ([ProveedorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ComprasProveedores_RecepcionCompraId] ON [ComprasProveedores] ([RecepcionCompraId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CuentasContables_Codigo] ON [CuentasContables] ([Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CuentasContables_CuentaPadreId] ON [CuentasContables] ([CuentaPadreId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DetallesAsientoContable_AsientoContableId] ON [DetallesAsientoContable] ([AsientoContableId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DetallesAsientoContable_CuentaContableId] ON [DetallesAsientoContable] ([CuentaContableId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DetallesOrdenCompra_OrdenCompraId] ON [DetallesOrdenCompra] ([OrdenCompraId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DetallesOrdenCompra_ProductoId] ON [DetallesOrdenCompra] ([ProductoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DetallesRecepcionCompra_DetalleOrdenCompraId] ON [DetallesRecepcionCompra] ([DetalleOrdenCompraId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DetallesRecepcionCompra_ProductoId] ON [DetallesRecepcionCompra] ([ProductoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DetallesRecepcionCompra_RecepcionCompraId] ON [DetallesRecepcionCompra] ([RecepcionCompraId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DetallesVenta_ProductoId] ON [DetallesVenta] ([ProductoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DetallesVenta_VentaId] ON [DetallesVenta] ([VentaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_KardexInventario_ProductoId] ON [KardexInventario] ([ProductoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LineasCompraProveedor_FacturaCompraId] ON [LineasCompraProveedor] ([FacturaCompraId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LineasCompraProveedor_ProductoId] ON [LineasCompraProveedor] ([ProductoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrdenesCompraProveedor_ProveedorId] ON [OrdenesCompraProveedor] ([ProveedorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Productos_CategoriaId] ON [Productos] ([CategoriaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Productos_CuentaCostoVentaId] ON [Productos] ([CuentaCostoVentaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Productos_CuentaInventarioId] ON [Productos] ([CuentaInventarioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Productos_CuentaVentaId] ON [Productos] ([CuentaVentaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Productos_Sku] ON [Productos] ([Sku]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Proveedores_Rut] ON [Proveedores] ([Rut]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RecepcionesMercaderia_OrdenCompraId] ON [RecepcionesMercaderia] ([OrdenCompraId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RolPermisos_PermisoId] ON [RolPermisos] ([PermisoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Usuarios_Email] ON [Usuarios] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Usuarios_RolId] ON [Usuarios] ([RolId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_VentasClientes_AsientoCostoVentaId] ON [VentasClientes] ([AsientoCostoVentaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_VentasClientes_AsientoVentaId] ON [VentasClientes] ([AsientoVentaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_VentasClientes_ClienteId] ON [VentasClientes] ([ClienteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928123828_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260928123828_InitialCreate', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022152519_AgregarPagosProveedoresPeriodosMatriz'
)
BEGIN
    ALTER TABLE [VentasClientes] ADD [AnuladoPor] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022152519_AgregarPagosProveedoresPeriodosMatriz'
)
BEGIN
    ALTER TABLE [VentasClientes] ADD [FechaAnulacion] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022152519_AgregarPagosProveedoresPeriodosMatriz'
)
BEGIN
    ALTER TABLE [VentasClientes] ADD [MotivoAnulacion] nvarchar(300) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022152519_AgregarPagosProveedoresPeriodosMatriz'
)
BEGIN
    ALTER TABLE [Usuarios] ADD [PorcentajeComision] decimal(5,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022152519_AgregarPagosProveedoresPeriodosMatriz'
)
BEGIN
    ALTER TABLE [OrdenesCompraProveedor] ADD [AnuladoPor] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022152519_AgregarPagosProveedoresPeriodosMatriz'
)
BEGIN
    ALTER TABLE [OrdenesCompraProveedor] ADD [AprobadoPor] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022152519_AgregarPagosProveedoresPeriodosMatriz'
)
BEGIN
    ALTER TABLE [OrdenesCompraProveedor] ADD [FechaAnulacion] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022152519_AgregarPagosProveedoresPeriodosMatriz'
)
BEGIN
    ALTER TABLE [OrdenesCompraProveedor] ADD [FechaAprobacion] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022152519_AgregarPagosProveedoresPeriodosMatriz'
)
BEGIN
    ALTER TABLE [OrdenesCompraProveedor] ADD [MotivoAnulacion] nvarchar(300) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022152519_AgregarPagosProveedoresPeriodosMatriz'
)
BEGIN
    ALTER TABLE [ComprasProveedores] ADD [AnuladoPor] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022152519_AgregarPagosProveedoresPeriodosMatriz'
)
BEGIN
    ALTER TABLE [ComprasProveedores] ADD [FechaAnulacion] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022152519_AgregarPagosProveedoresPeriodosMatriz'
)
BEGIN
    ALTER TABLE [ComprasProveedores] ADD [MotivoAnulacion] nvarchar(300) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022152519_AgregarPagosProveedoresPeriodosMatriz'
)
BEGIN
    ALTER TABLE [ComprasProveedores] ADD [SaldoPendiente] decimal(18,4) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022152519_AgregarPagosProveedoresPeriodosMatriz'
)
BEGIN
    ALTER TABLE [ComprasProveedores] ADD [TotalPagado] decimal(18,4) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022152519_AgregarPagosProveedoresPeriodosMatriz'
)
BEGIN
    ALTER TABLE [AsientosContables] ADD [AsientoRevertidoId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022152519_AgregarPagosProveedoresPeriodosMatriz'
)
BEGIN
    CREATE TABLE [MatrizIntegracionContable] (
        [Id] int NOT NULL IDENTITY,
        [ModuloOrigen] nvarchar(50) NOT NULL,
        [TipoOperacion] nvarchar(100) NOT NULL,
        [CuentaDebeId] int NOT NULL,
        [CuentaHaberId] int NOT NULL,
        [Descripcion] nvarchar(300) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [CreadoPor] nvarchar(max) NULL,
        [ModificadoPor] nvarchar(max) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_MatrizIntegracionContable] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MatrizIntegracionContable_CuentasContables_CuentaDebeId] FOREIGN KEY ([CuentaDebeId]) REFERENCES [CuentasContables] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MatrizIntegracionContable_CuentasContables_CuentaHaberId] FOREIGN KEY ([CuentaHaberId]) REFERENCES [CuentasContables] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022152519_AgregarPagosProveedoresPeriodosMatriz'
)
BEGIN
    CREATE TABLE [PagosProveedores] (
        [Id] int NOT NULL IDENTITY,
        [CompraProveedorId] int NOT NULL,
        [FechaPago] datetime2 NOT NULL,
        [Monto] decimal(18,4) NOT NULL,
        [MetodoPago] nvarchar(max) NOT NULL,
        [ReferenciaComprobante] nvarchar(max) NULL,
        [AsientoPagoId] int NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [CreadoPor] nvarchar(max) NULL,
        [ModificadoPor] nvarchar(max) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_PagosProveedores] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PagosProveedores_AsientosContables_AsientoPagoId] FOREIGN KEY ([AsientoPagoId]) REFERENCES [AsientosContables] ([Id]),
        CONSTRAINT [FK_PagosProveedores_ComprasProveedores_CompraProveedorId] FOREIGN KEY ([CompraProveedorId]) REFERENCES [ComprasProveedores] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022152519_AgregarPagosProveedoresPeriodosMatriz'
)
BEGIN
    CREATE TABLE [PeriodosContables] (
        [Id] int NOT NULL IDENTITY,
        [Anio] int NOT NULL,
        [Mes] int NOT NULL,
        [FechaInicio] datetime2 NOT NULL,
        [FechaFin] datetime2 NOT NULL,
        [Estado] int NOT NULL,
        [FechaCierre] datetime2 NULL,
        [CerradoPor] nvarchar(max) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [CreadoPor] nvarchar(max) NULL,
        [ModificadoPor] nvarchar(max) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_PeriodosContables] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022152519_AgregarPagosProveedoresPeriodosMatriz'
)
BEGIN
    CREATE INDEX [IX_AsientosContables_AsientoRevertidoId] ON [AsientosContables] ([AsientoRevertidoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022152519_AgregarPagosProveedoresPeriodosMatriz'
)
BEGIN
    CREATE INDEX [IX_MatrizIntegracionContable_CuentaDebeId] ON [MatrizIntegracionContable] ([CuentaDebeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022152519_AgregarPagosProveedoresPeriodosMatriz'
)
BEGIN
    CREATE INDEX [IX_MatrizIntegracionContable_CuentaHaberId] ON [MatrizIntegracionContable] ([CuentaHaberId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022152519_AgregarPagosProveedoresPeriodosMatriz'
)
BEGIN
    CREATE UNIQUE INDEX [IX_MatrizIntegracionContable_ModuloOrigen_TipoOperacion] ON [MatrizIntegracionContable] ([ModuloOrigen], [TipoOperacion]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022152519_AgregarPagosProveedoresPeriodosMatriz'
)
BEGIN
    CREATE INDEX [IX_PagosProveedores_AsientoPagoId] ON [PagosProveedores] ([AsientoPagoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022152519_AgregarPagosProveedoresPeriodosMatriz'
)
BEGIN
    CREATE INDEX [IX_PagosProveedores_CompraProveedorId] ON [PagosProveedores] ([CompraProveedorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022152519_AgregarPagosProveedoresPeriodosMatriz'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PeriodosContables_Anio_Mes] ON [PeriodosContables] ([Anio], [Mes]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022152519_AgregarPagosProveedoresPeriodosMatriz'
)
BEGIN
    ALTER TABLE [AsientosContables] ADD CONSTRAINT [FK_AsientosContables_AsientosContables_AsientoRevertidoId] FOREIGN KEY ([AsientoRevertidoId]) REFERENCES [AsientosContables] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022152519_AgregarPagosProveedoresPeriodosMatriz'
)
BEGIN
    EXEC(N'UPDATE ComprasProveedores SET SaldoPendiente = CASE WHEN Estado = 3 THEN 0 ELSE Total END, TotalPagado = 0;')
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022152519_AgregarPagosProveedoresPeriodosMatriz'
)
BEGIN
    EXEC(N'
    INSERT INTO MatrizIntegracionContable (ModuloOrigen, TipoOperacion, CuentaDebeId, CuentaHaberId, Descripcion, FechaCreacion, CreadoPor, Activo)
    SELECT v.Modulo, v.Operacion, d.Id, h.Id, v.Descripcion, SYSUTCDATETIME(), ''System'', 1
    FROM (VALUES
     (N''Compras'', N''FacturaCompraNeto'', N''1.1.03.01'', N''2.1.01.01'', N''Factura de compra: ingreso de mercadería (neto) contra cuenta por pagar''),
     (N''Compras'', N''FacturaCompraIva'', N''1.1.04.01'', N''2.1.01.01'', N''Factura de compra: IVA crédito fiscal contra cuenta por pagar''),
     (N''Compras'', N''PagoProveedorEfectivo'', N''2.1.01.01'', N''1.1.01.01'', N''Pago a proveedor en efectivo (sale de Caja)''),
     (N''Compras'', N''PagoProveedorBanco'', N''2.1.01.01'', N''1.1.01.02'', N''Pago a proveedor por transferencia o tarjeta (sale de Banco)''),
     (N''Ventas'', N''VentaContadoEfectivo'', N''1.1.01.01'', N''4.1.01.01'', N''Venta al contado en efectivo: cobro contra ingreso (neto)''),
     (N''Ventas'', N''VentaContadoBanco'', N''1.1.01.02'', N''4.1.01.01'', N''Venta con tarjeta/transferencia: cobro contra ingreso (neto)''),
     (N''Ventas'', N''VentaCredito'', N''1.1.02.01'', N''4.1.01.01'', N''Venta a crédito: cuenta por cobrar contra ingreso (neto)''),
     (N''Ventas'', N''IvaDebitoVenta'', N''1.1.02.01'', N''2.1.02.01'', N''IVA débito de la venta (el Debe real es la cuenta de cobro de la venta: Caja, Banco o Clientes)''),
     (N''Ventas'', N''CostoVenta'', N''5.1.01.01'', N''1.1.03.01'', N''Costo de ventas (CPP) contra salida de inventario''),
     (N''Ventas'', N''AbonoClienteEfectivo'', N''1.1.01.01'', N''1.1.02.01'', N''Abono de cliente en efectivo''),
     (N''Ventas'', N''AbonoClienteBanco'', N''1.1.01.02'', N''1.1.02.01'', N''Abono de cliente por transferencia o tarjeta''),
     (N''Inventario'', N''AjusteMerma'', N''6.1.01.01'', N''1.1.03.01'', N''Ajuste de inventario negativo (merma/pérdida)''),
     (N''Inventario'', N''AjusteSobrante'', N''1.1.03.01'', N''6.1.01.01'', N''Ajuste de inventario positivo (sobrante)'')
    ) AS v(Modulo, Operacion, CodDebe, CodHaber, Descripcion)
    JOIN CuentasContables d ON d.Codigo = v.CodDebe
    JOIN CuentasContables h ON h.Codigo = v.CodHaber
    WHERE NOT EXISTS (SELECT 1 FROM MatrizIntegracionContable m WHERE m.ModuloOrigen = v.Modulo AND m.TipoOperacion = v.Operacion);')
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022152519_AgregarPagosProveedoresPeriodosMatriz'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261022152519_AgregarPagosProveedoresPeriodosMatriz', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029140856_EliminarGiroCliente'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clientes]') AND [c].[name] = N'Giro');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Clientes] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [Clientes] DROP COLUMN [Giro];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029140856_EliminarGiroCliente'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261029140856_EliminarGiroCliente', N'10.0.11');
END;

COMMIT;
GO

