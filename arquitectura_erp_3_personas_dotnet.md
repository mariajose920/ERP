**Arquitectura ERP Integrado**

**Guía de desarrollo para un equipo de 3 personas con .NET**

**Objetivo**

Desarrollar un ERP web centralizado que integre Compras, Ventas, Inventario y Contabilidad, con autenticación, control de acceso por roles e intercambio de información entre módulos.

La solución estará construida con [**ASP.NET**](http://ASP.NET) **Core** en el backend, **SQL Server en SQL Server** como base de datos central y un repositorio único en **GitHub**. El sistema se ejecuta de forma local (arquitectura on-premise, Kestrel + SQL Server Express) dentro de la red de la PyME; el equipo comparte el código mediante GitHub. El frontend carga React y Tailwind por CDN, por lo que el primer arranque de cada navegador requiere conexión a Internet (puede eliminarse alojando esos archivos localmente).

**1\. Arquitectura tecnológica propuesta**

**Backend**

* [ASP.NET](http://ASP.NET) Core Web API con .NET.

* Entity Framework Core para el acceso a datos.

* SQL Server como motor de base de datos.

* Autenticación con un servicio propio (AuthService): contraseñas con PBKDF2-SHA256 (100.000 iteraciones y sal por usuario) y sesiones con JWT de 12 horas.

* API REST para la comunicación entre el frontend y los módulos.

* Middleware para autenticación y autorización basada en roles y permisos.

* Swagger/OpenAPI para documentar y probar los endpoints.

**Base de datos y servicios**

* SQL Server alojado en SQL Server.

* implementaciones de Entity Framework Core para controlar la estructura de la base de datos.

* Restricciones, claves foráneas y transacciones para proteger la consistencia de las operaciones.

* Registro de auditoría para operaciones relevantes, como creación, modificación,  anulación y ajustes.

**Control de versiones y colaboración**

* Repositorio único en GitHub.

* Ramas por funcionalidad o módulo.

* Pull Requests para revisar cambios antes de integrarlos a la rama principal.

* Issues y tablero de proyecto para organizar tareas.

* Archivo README con instrucciones de instalación, configuración y ejecución.

**Despliegue**

* Backend ejecutado en el servidor local de la empresa (Kestrel), accesible por intranet.

* Variables de entorno para las cadenas de conexión, claves JWT y configuraciones sensibles.

* Frontend servido por la misma API (carpeta wwwroot), por lo que consume la API en el mismo origen.

* Separación de ambientes de desarrollo y producción cuando sea posible.

**2\. Distribución del trabajo**

La distribución se adapta a tres personas. Cristobal asume el módulo de Inventario y Contabilidad, además de la integración general y el control de accesos, porque estos componentes conectan los movimientos de Compras y Ventas con el stock y los asientos contables. Cada integrante entrega **5 pantallas, 5 reportes y 10 requerimientos funcionales** (30 RF en total, detallados en el capítulo 6.3.1 del documento del Trabajo Final).

| Integrante | Responsabilidad principal | Alcance |
| :---- | :---- | :---- |
| Cristobal | Contabilidad, Inventario e Integración/RBAC | Catálogo de productos, stock, kardex, CPP, ajustes, contabilidad, autenticación, roles, permisos e integración entre módulos |
| Adan | Compras | Proveedores, órdenes de compra (con aprobación), recepción de mercadería, facturas de compra, historial de compras, reportes de abastecimiento y generación del asiento correspondiente |
| Maria | Ventas | Clientes, punto de venta con validación de stock y crédito, caja diaria, cobranzas con abono en cascada, historial de ventas, anulaciones y reportes (incluye comisiones sugeridas) |

**Responsabilidades compartidas**

* Definir convenciones de nombres para clases, endpoints, tablas y campos.

* Revisar los cambios mediante Pull Requests.

* Crear pruebas para los procesos críticos.

* Documentar los endpoints y las reglas de negocio.

* Validar la integración completa antes de la presentación.

**3\. Módulo de Compras: Adan**

**Pantallas sugeridas**

1. **Gestión de Proveedores** (pantalla_gestion_proveedores.html): CRUD de proveedores, incluyendo RUT, razón social, contacto, dirección, correo, teléfono y condiciones de pago.

2. **Órdenes de Compra** (pantalla_ordenes_compra.html): creación, aprobación, anulación e historial de órdenes con detalle de productos, cantidades, precios y estado.

3. **Recepción de Mercadería** (pantalla_recepcion_mercaderia.html): registro de recepciones totales o parciales asociadas a una orden aprobada, con la diferencia pedido vs recibido.

4. **Factura de Compra** (pantalla_facturas_compra.html): ingreso y aprobación de la factura, validación de cantidades y precios, y asociación con el proveedor y la orden de compra.

5. **Historial y Detalle de Compras** (pantalla_historial_compras.html): consulta de los productos recibidos, diferencias, impuestos, totales, estado de liquidación y trazabilidad por orden.

**Reglas de negocio**

* Una orden de compra puede estar pendiente de aprobación, aprobada, recibida parcialmente, recibida completamente, liquidada o anulada. Solo una orden aprobada admite recepciones.

* La recepción no debe permitir cantidades superiores a las pendientes de la orden.

* Una factura de compra aprobada debe aumentar el stock mediante el módulo de Inventario.

* La factura aprobada debe generar un asiento contable con el esquema general: **Debe: Inventario o cuenta de compra / Haber: Proveedores**.

* Las anulaciones deben realizarse mediante operaciones compensatorias o reversas auditables, evitando eliminar transacciones aprobadas.

**Reportes sugeridos**

Los cinco reportes están en pantalla_reportes_compras.html:

* Compras por proveedor, filtradas por rango de fechas.

* Historial de precios de compra por producto.

* Cumplimiento de órdenes de compra: pedido vs recibido vs facturado, con alerta de atraso.

* Consolidado de facturas de compra, filtrable por proveedor y estado.

* Cuentas por pagar: facturas aprobadas con vencimiento y días de mora.

**API sugerida**

* GET /api/proveedores

* POST /api/proveedores

* PUT /api/proveedores/{id}

* DELETE /api/proveedores/{id}

* GET /api/compras/ordenes

* POST /api/compras/ordenes

* POST /api/compras/ordenes/{id}/aprobar

* POST /api/compras/ordenes/{id}/anular

* POST /api/compras/recepciones

* POST /api/compras/facturas

* GET /api/reportes/compras-por-proveedor

* GET /api/reportes/historial-precios-compra

* GET /api/reportes/cumplimiento-ordenes

* GET /api/reportes/consolidado-facturas-compra

* GET /api/reportes/cuentas-por-pagar

**4\. Módulo de Ventas: Maria**

**Pantallas sugeridas**

1. **Caja del día** (pantalla_caja_inicio.html): ventas y abonos propios del día con cierre imprimible.

2. **Emisión de Comprobantes / POS** (pantalla_emision_ventas.html): venta con selección de cliente, productos, cantidades, precios, descuentos, impuestos y forma de pago; valida stock y crédito antes de confirmar.

3. **Gestión de Clientes** (pantalla_gestion_clientes.html): datos comerciales, RUT, razón social, dirección, contacto, límite de crédito y estado.

4. **Cobranzas a plazo** (pantalla_cobranzas_plazo.html): abonos individuales y abonos en cascada sobre las facturas más antiguas.

5. **Historial de Ventas** (pantalla_historial_directas.html): monitor de documentos emitidos con estados, detalle de la venta, trazabilidad y anulación.

**Reglas de negocio**

* No se debe confirmar una venta si la cantidad solicitada supera el stock disponible.

* Una factura emitida debe descontar existencias mediante el módulo de Inventario.

* La venta debe generar los asientos contables correspondientes: reconocimiento de la venta y reconocimiento del costo de venta.

* Una anulación debe devolver el stock y generar los asientos de reversa que correspondan.

* Los documentos deben mantener un historial de cambios y usuario responsable.

**Reportes sugeridos**

Los reportes están en pantalla_reportes_ventas.html y pantalla_cobranzas_ventas.html:

* Resumen de ventas: indicadores del día y del mes, ticket promedio y ventas diarias.

* Ranking de ventas por cliente, con montos acumulados.

* Ventas por producto, con unidades, ingreso neto y margen.

* Cuentas por cobrar: facturas pendientes, saldo, fecha de vencimiento y días de mora.

* Comisiones sugeridas por vendedor sobre el neto vendido, con porcentaje configurable.

**API sugerida**

* GET /api/clientes

* POST /api/clientes

* PUT /api/clientes/{id}

* GET /api/ventas

* POST /api/ventas

* GET /api/ventas/{id}

* POST /api/ventas/{id}/anular

* GET /api/reportes/ventas-por-cliente

* GET /api/reportes/ventas-por-producto

* GET /api/reportes/cuentas-por-cobrar

* GET /api/reportes/resumen-ventas

* GET /api/reportes/comisiones-ventas

**5\. Módulo de Inventario: Cristobal**

**Pantallas sugeridas**

1. **Catálogo de Productos** (pantalla_catalogo_productos.html): CRUD de SKU, nombre, categoría, unidad de medida, precio de venta, costo base, stock mínimo y punto de reorden; muestra stock y CPP vigente.

2. **Kardex** (pantalla_kardex_inventario.html): trazabilidad de entradas, salidas, ajustes, documento de origen, usuario, fecha y saldo resultante.

3. **Ajustes y Alertas** (pantalla_ajustes_alertas_inventario.html): ajustes manuales con motivo (mermas, pérdidas, daños, correcciones) y listado de productos bajo el punto de reorden.

4. **Valorización de Inventario** (pantalla_valorizacion_inventario.html): stock multiplicado por el CPP, por producto y por categoría.

El **Costo Promedio Ponderado** se recalcula automáticamente cada vez que se aprueba una factura de compra.

**Reglas de negocio**

* Cada entrada o salida debe generar un movimiento de kardex.

* Las entradas provenientes de compras deben registrar cantidad, costo unitario y documento de origen.

* Las salidas provenientes de ventas deben utilizar el costo vigente para determinar el costo de venta.

* El CPP puede calcularse mediante la fórmula:

$$CPP$$

* Los ajustes manuales deben exigir motivo y usuario responsable.

* El stock no debe quedar negativo, salvo que exista una regla explícita aprobada para el proyecto.

**Reportes sugeridos**

* Valorización del inventario: stock actual multiplicado por el CPP.

* Kardex detallado por producto y rango de fechas.

* Alertas de stock mínimo y punto de reorden.

* Historial de ajustes, mermas, pérdidas y daños (en la pantalla de ajustes).

**API sugerida**

* GET /api/productos

* POST /api/productos

* PUT /api/productos/{id}

* GET /api/inventario/stock

* GET /api/inventario/kardex/{productoId}

* POST /api/inventario/ajustes

* GET /api/inventario/alertas-reorden

* GET /api/inventario/valorizacion

* GET /api/inventario/ajustes

**6\. Módulo de Contabilidad: Cristobal**

**Pantallas sugeridas**

1. **Plan de Cuentas:** estructura jerárquica de cuentas contables con código, nombre, tipo, naturaleza y estado.

2. **Libro Diario:** consulta y registro de asientos manuales y automáticos generados por Compras, Ventas e Inventario.

3. **Libro Mayor:** movimientos agrupados por cuenta y período, con débitos, créditos y saldo.

4. **Balance de Comprobación** (pantalla_reportes_contabilidad.html): vista de saldos para comprobar el cuadre contable.

5. **Estados Financieros** (pantalla_estados_financieros.html): estado de resultados (ingresos menos costo de ventas y gastos) y balance general.

6. **Configuración de Integración** (pantalla_configuracion_integracion.html): asociación entre eventos del ERP y cuentas contables, y estado de la centralización.

**Asientos automáticos principales**

**Compra aprobada**

* Debe: Inventario o cuenta de compras.

* Haber: Proveedores.

**Venta emitida**

* Debe: Clientes, Caja o Banco.

* Haber: Ventas e impuestos por pagar, según corresponda.

**Costo de venta**

* Debe: Costo de Ventas.

* Haber: Inventario.

**Pago de cliente**

* Debe: Caja o Banco.

* Haber: Clientes.

**Reglas de negocio**

* Todo asiento debe estar cuadrado: total de débitos igual a total de créditos.

* Los asientos automáticos deben guardar el módulo y documento que los originó.

* Los períodos cerrados no deben permitir modificaciones directas.

* Las correcciones deben realizarse mediante asientos de reversa o ajustes autorizados.

* Las cuentas contables utilizadas por la integración deben existir y estar activas.

**Reportes sugeridos**

* Balance de comprobación de ocho columnas.

* Estado de resultados.

* Libro Mayor centralizado.

* Libro Diario filtrado por período, módulo, cuenta y tipo de asiento.

* Conciliación entre movimientos de inventario, ventas, compras y contabilidad.

**API sugerida**

* GET /api/maestros/cuentas-contables

* POST /api/maestros/cuentas-contables

* POST /api/contabilidad/asientos

* GET /api/contabilidad/libro-diario

* GET /api/contabilidad/libro-mayor/{cuentaId}

* GET /api/contabilidad/balance-comprobacion

* GET /api/contabilidad/estado-resultados

* GET /api/contabilidad/balance-general

**7\. Integración y control de accesos: Cristobal**

**Portal maestro de navegación**

El sistema debe contar con un portal unificado después del inicio de sesión. El menú se genera dinámicamente según el rol y los permisos del usuario autenticado.

El portal debe incluir:

* Inicio o panel principal.

* Acceso a Compras.

* Acceso a Ventas.

* Acceso a Inventario.

* Acceso a Contabilidad.

* Administración de usuarios y perfiles, únicamente para administradores.

* Reportes disponibles según los permisos del usuario.

* Auditoría de eventos con filtros (pantalla_auditoria.html), únicamente para administradores.

**Administración de usuarios y perfiles**

* Registro y edición de usuarios.

* Contraseñas almacenadas de forma segura con PBKDF2-SHA256 y sal por usuario.

* Asociación de usuarios a roles.

* Activación y desactivación de cuentas.

* Registro del último acceso.

* Recuperación y cambio de contraseña.

**RBAC: roles y permisos**

El control de acceso debe aplicarse tanto en la interfaz como en los endpoints del backend. Ocultar una opción del menú no reemplaza la autorización del servidor.

| Rol | Compras | Ventas | Inventario | Contabilidad | Administración |
| :---- | :---- | :---- | :---- | :---- | :---- |
| Administrador | Total | Total | Total | Total | Total |
| Cajero | Sin acceso | Solo POS, clientes básicos, abonos y caja propia | Sin acceso | Sin acceso | Sin acceso |
| Comprador | Lectura y escritura | Sin acceso | Solo consulta | Sin acceso | Sin acceso |
| Vendedor | Sin acceso | Lectura y escritura | Solo consulta | Sin acceso | Sin acceso |
| Bodeguero | Solo consulta | Sin acceso | Lectura y escritura | Sin acceso | Sin acceso |
| Contador | Solo consulta | Solo consulta | Solo consulta | Lectura y escritura | Sin acceso |

**Permisos recomendados**

Además de los roles, se pueden definir permisos específicos:

* compras.proveedores.leer

* compras.proveedores.escribir

* compras.ordenes.leer

* compras.ordenes.escribir

* compras.recepciones.crear

* ventas.clientes.leer

* ventas.documentos.crear

* ventas.documentos.anular

* inventario.productos.escribir

* inventario.stock.leer

* inventario.ajustes.crear

* contabilidad.asientos.leer

* contabilidad.asientos.crear

* contabilidad.reportes.leer

* usuarios.administrar

**Implementación sugerida en [ASP.NET](http://ASP.NET) Core**

* Usar \[Authorize\] en controladores o endpoints.

* Usar políticas con AddAuthorization para permisos específicos.

* Usar roles para restricciones generales.

* Validar el usuario y sus permisos en cada operación sensible.

* Registrar intentos de acceso denegados.

* No incluir secretos ni claves privadas en el repositorio.

**8\. Flujo transaccional integrado**

El flujo principal del ERP debe funcionar de la siguiente manera:

**1\. Compra**

Adan registra y aprueba una factura de compra asociada a una orden de compra. El sistema comunica la operación al módulo de Inventario y al módulo de Contabilidad.

**2\. Existencias**

Cristobal registra la entrada en el kardex, actualiza el stock y recalcula el Costo Promedio Ponderado. La operación queda vinculada a la factura de compra.

**3\. Asiento de compra**

Contabilidad genera el asiento correspondiente, por ejemplo: **Debe: Inventario / Haber: Proveedores**.

**4\. Venta**

Maria emite una factura. El sistema valida que exista stock suficiente y confirma la operación solo si la validación es exitosa.

**5\. Salida de inventario**

Inventario descuenta las unidades vendidas, registra la salida en el kardex y determina el costo de venta usando el CPP vigente.

**6\. Asientos de venta**

Contabilidad genera el asiento de la venta y el asiento del costo de venta.

**7\. Reportes**

Los movimientos consolidados permiten emitir el Libro Diario, Libro Mayor, balance de comprobación, estado de resultados, reportes de inventario, compras y ventas.

**Consistencia transaccional**

Las operaciones que actualizan varios módulos deben ejecutarse dentro de una transacción. Si falla la actualización del stock o la generación del asiento, la operación completa debe revertirse para evitar diferencias entre Compras, Ventas, Inventario y Contabilidad.

**9\. Modelo de datos inicial**

Las entidades principales pueden organizarse de la siguiente forma:

**Seguridad**

* AspNetUsers.

* AspNetRoles.

* AspNetUserRoles.

* Permisos.

* RolPermisos.

* Auditoria.

**Maestros**

* Productos.

* Categorias.

* Proveedores.

* Clientes.

* CuentasContables.  
  .

**Compras**

* OrdenesCompra.

* DetalleOrdenCompra.

* RecepcionesCompra.

* DetalleRecepcionCompra.

* FacturasCompra.

* DetalleFacturaCompra.

**Ventas**

* Ventas o FacturasVenta.

* DetalleVenta.

* PagosVenta.  
  .

**Inventario**

* Existencias.

* MovimientosKardex.

* AjustesInventario.

**Contabilidad**

* AsientosContables.

* DetalleAsientoContable.  
  .

**Relaciones clave**

* Una orden de compra pertenece a un proveedor y posee varios detalles.

* Una recepción se relaciona con una orden de compra.

* Una factura de compra puede relacionarse con una recepción y un proveedor.

* Una venta pertenece a un cliente y posee varios detalles.

* Cada movimiento de kardex debe registrar su documento de origen.

* Cada asiento contable debe registrar el módulo y documento que lo generó.

* Los productos deben relacionarse con una cuenta de inventario y, cuando corresponda, con cuentas de ventas y costo de ventas.

**10\. Organización del repositorio .NET**

ERPIntegrado/  
├── src/  
│   ├── ERP.Api/  
│   ├── ERP.Application/  
│   ├── ERP.Domain/  
│   ├── ERP.Infrastructure/  
│   └── ERP.Web/                 \# Si se utiliza frontend Blazor o ASP.NET Core  
├── tests/  
│   ├── ERP.UnitTests/  
│   └── ERP.IntegrationTests/  
├── database/  
│   └── scripts/  
├── docs/  
├── .gitignore  
├── README.md  
└── ERPIntegrado.sln

**Capas sugeridas**

* **ERP.Domain:** entidades, enumeraciones, reglas y contratos principales.

* **ERP.Application:** casos de uso, DTO, validaciones y servicios de aplicación.

* **ERP.Infrastructure:** Entity Framework Core, SQL Server, Identity y servicios externos.

* **ERP.Api:** controladores, autenticación, autorización, filtros y configuración HTTP.

* [**ERP.Web**](http://ERP.Web)**:** interfaz web, si el equipo utiliza Blazor, Razor Pages u otro frontend conectado a la API.

**11\. Plan de trabajo por etapas**

**Etapa 1: Base común**

* Crear la solución .NET y los proyectos por capas.

* Configurar GitHub y las ramas de trabajo.

* Configurar SQL Server y la conexión SQL Server.

* Implementar Identity, JWT y roles iniciales.

* Definir entidades compartidas y implementaciones.

**Etapa 2: Maestros**

* Crear productos, categorías, proveedores y clientes.

* Implementar validaciones, búsqueda y estados activos/inactivos.

* Crear permisos básicos para cada módulo.

**Etapa 3: Compras y Ventas**

* Adan desarrolla proveedores, órdenes, recepciones y facturas de compra.

* Maria desarrolla clientes, ventas, facturación e historial.

* Ambos deben consumir el catálogo central de productos.

**Etapa 4: Inventario y Contabilidad**

* Cristobal desarrolla kardex, stock, ajustes y CPP.

* Cristobal desarrolla plan de cuentas, asientos, Libro Diario y reportes.

* Se integran las entradas de compras y salidas de ventas.

**Etapa 5: Integración**

* Conectar la aprobación de compras con Inventario y Contabilidad.

* Conectar la emisión de ventas con Inventario y Contabilidad.

* Aplicar transacciones y validaciones de consistencia.

* Completar el portal maestro y la matriz RBAC.

**Etapa 6: Pruebas y presentación**

* Probar cada endpoint con Swagger o Postman.

* Ejecutar pruebas de permisos por rol.

* Validar el flujo completo: compra, entrada, venta, salida y balance.

* Verificar que los débitos y créditos cuadren.

* Preparar datos de demostración y documentación técnica.

**12\. Reportes finales del ERP**

**Compras**

* Compras por proveedor.

* Cumplimiento de órdenes de compra.

* Historial de precios de insumos.

* Consolidado de facturas de compra.

**Ventas**

* Ranking de clientes.

* Ventas por producto y categoría.

* Facturas pendientes de cobro.

* Historial de ventas por estado.

**Inventario**

* Valorización de stock mediante CPP.

* Kardex por producto.

* Rotación de productos.

* Mermas, pérdidas y daños.

* Alertas de stock mínimo y reorden.

**Contabilidad**

* Balance de comprobación de ocho columnas.

* Estado de resultados.

* Libro Diario.

* Libro Mayor centralizado.

* Conciliación entre documentos operacionales y asientos contables.

**13\. Criterios de aceptación**

El proyecto se considerará funcional cuando:

* Los tres integrantes puedan trabajar sobre el mismo repositorio remoto.

* El backend [ASP.NET](http://ASP.NET) Core pueda conectarse correctamente a SQL Server en SQL Server.

* Los usuarios puedan iniciar sesión y acceder solo a los módulos permitidos.

* Adan pueda registrar una compra y aprobar su recepción o factura.

* La compra aprobada incremente el stock y genere el asiento contable correspondiente.

* Maria pueda emitir una venta únicamente cuando exista stock suficiente.

* La venta descuente stock, registre el kardex y genere los asientos de venta y costo de venta.

* Cristobal pueda consultar el Libro Diario, Libro Mayor, balance y estado de resultados.

* Los reportes permitan filtrar información relevante por fechas, productos, clientes, proveedores y estados.

* Las operaciones integradas utilicen transacciones y mantengan consistencia entre módulos.

* El sistema esté disponible mediante una URL pública para pruebas y presentación.

**Conclusión**

La adaptación a tres personas concentra en Cristobal las funciones que requieren mayor coordinación transversal: Inventario, Contabilidad, Integración y RBAC. Adan se enfoca en el ciclo de abastecimiento y Maria en el ciclo comercial. Con un backend centralizado en [ASP.NET](http://ASP.NET) Core, SQL Server en SQL Server, un repositorio único en GitHub y despliegue accesible por Internet, el equipo puede desarrollar en paralelo y mantener todos los módulos conectados en una única solución ERP.