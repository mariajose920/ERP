**Arquitectura ERP Integrado**

**Guía de desarrollo para un equipo de 3 personas con .NET**

**Objetivo**

Desarrollar un ERP web centralizado que integre Compras, Ventas, Inventario y Contabilidad, con autenticación, control de acceso por roles e intercambio de información entre módulos.

La solución estará construida con [**ASP.NET**](http://ASP.NET) **Core** en el backend, **PostgreSQL en Supabase** como base de datos central y un repositorio único en **GitHub**. El sistema se desplegará en un hosting accesible por Internet para que los tres integrantes puedan trabajar desde distintos computadores y ubicaciones sin depender de una red local compartida.

**1\. Arquitectura tecnológica propuesta**

**Backend**

* [ASP.NET](http://ASP.NET) Core Web API con .NET.

* Entity Framework Core para el acceso a datos.

* PostgreSQL como motor de base de datos.

* Autenticación mediante [ASP.NET](http://ASP.NET) Core Identity y JWT.

* API REST para la comunicación entre el frontend y los módulos.

* Middleware para autenticación y autorización basada en roles y permisos.

* Swagger/OpenAPI para documentar y probar los endpoints.

**Base de datos y servicios**

* PostgreSQL alojado en Supabase.

* Migraciones de Entity Framework Core para controlar la estructura de la base de datos.

* Restricciones, claves foráneas y transacciones para proteger la consistencia de las operaciones.

* Registro de auditoría para operaciones relevantes, como creación, modificación,  anulación y ajustes.

**Control de versiones y colaboración**

* Repositorio único en GitHub.

* Ramas por funcionalidad o módulo.

* Pull Requests para revisar cambios antes de integrarlos a la rama principal.

* Issues y tablero de proyecto para organizar tareas.

* Archivo README con instrucciones de instalación, configuración y ejecución.

**Despliegue**

* Backend publicado en un hosting accesible desde Internet.

* Variables de entorno para las cadenas de conexión, claves JWT y configuraciones sensibles.

* Frontend configurado para consumir la URL pública de la API.

* Separación de ambientes de desarrollo y producción cuando sea posible.

**2\. Distribución del trabajo**

La distribución se adapta a tres personas. Cristobal asume el módulo de Inventario y Contabilidad, además de la integración general y el control de accesos, porque estos componentes conectan los movimientos de Compras y Ventas con el stock y los asientos contables.

| Integrante | Responsabilidad principal | Alcance |
| :---- | :---- | :---- |
| Cristobal | Contabilidad, Inventario e Integración/RBAC | Catálogo de productos, stock, kardex, CPP, ajustes, contabilidad, autenticación, roles, permisos e integración entre módulos |
| Adan | Compras | Proveedores, órdenes de compra, recepción de mercadería, facturas de compra y generación del asiento correspondiente |
| Maria | Ventas | Clientes, facturación con validación de stock, historial de ventas y estados de documentos |

**Responsabilidades compartidas**

* Definir convenciones de nombres para clases, endpoints, tablas y campos.

* Revisar los cambios mediante Pull Requests.

* Crear pruebas para los procesos críticos.

* Documentar los endpoints y las reglas de negocio.

* Validar la integración completa antes de la presentación.

**3\. Módulo de Compras: Adan**

**Pantallas sugeridas**

1. **Gestión de Proveedores:** CRUD completo de proveedores, incluyendo RUT, razón social, contacto, dirección, correo, teléfono y condiciones de pago.

2. **Órdenes de Compra:** creación, edición controlada e historial de órdenes con detalle de productos, cantidades, precios y estado.

3. **Recepción de Mercadería:** registro de recepciones totales o parciales asociadas a una orden de compra.

4. **Factura de Compra:** ingreso de la factura, validación de cantidades y precios, y asociación con el proveedor y la orden de compra.

5. **Detalle de Compra:** consulta de los productos recibidos, diferencias, impuestos, totales y estado de liquidación.

**Reglas de negocio**

* Una orden de compra puede estar pendiente, recibida parcialmente, recibida completamente, anulada o liquidada.

* La recepción no debe permitir cantidades superiores a las pendientes de la orden.

* Una factura de compra aprobada debe aumentar el stock mediante el módulo de Inventario.

* La factura aprobada debe generar un asiento contable con el esquema general: **Debe: Inventario o cuenta de compra / Haber: Proveedores**.

* Las anulaciones deben realizarse mediante operaciones compensatorias o reversas auditables, evitando eliminar transacciones aprobadas.

**Reportes sugeridos**

* Compras por proveedor, filtradas por rango de fechas y RUT.

* Consolidado de facturas de compra ingresadas.

* Cumplimiento de órdenes de compra: pendientes, recibidas parcialmente y liquidadas.

* Historial de precios de compra por producto.

**API sugerida**

* GET /api/proveedores

* POST /api/proveedores

* PUT /api/proveedores/{id}

* DELETE /api/proveedores/{id}

* GET /api/ordenes-compra

* POST /api/ordenes-compra

* GET /api/ordenes-compra/{id}

* POST /api/recepciones-compra

* POST /api/facturas-compra

* GET /api/reportes/compras-por-proveedor

**4\. Módulo de Ventas: Maria**

**Pantallas sugeridas**

1. **Gestión de Clientes:** formulario de datos comerciales, RUT, razón social o nombre, dirección, contacto, límite de crédito y estado.

2. **Emisión de Comprobantes:** pantalla de venta o facturación con selección de cliente, productos, cantidades, precios, descuentos, impuestos y forma de pago.

3. **Validación de Stock:** consulta de existencias antes de confirmar la venta y bloqueo de cantidades que superen el stock disponible.

4. **Historial de Ventas:** monitor de documentos emitidos con estados como pagada, pendiente, anulada y parcialmente pagada.

5. **Detalle de Venta:** visualización de productos vendidos, totales, pagos, cliente y trazabilidad de la operación.

**Reglas de negocio**

* No se debe confirmar una venta si la cantidad solicitada supera el stock disponible.

* Una factura emitida debe descontar existencias mediante el módulo de Inventario.

* La venta debe generar los asientos contables correspondientes: reconocimiento de la venta y reconocimiento del costo de venta.

* Una anulación debe devolver el stock y generar los asientos de reversa que correspondan.

* Los documentos deben mantener un historial de cambios y usuario responsable.

**Reportes sugeridos**

* Ranking de ventas por cliente, con montos acumulados y participación porcentual.

* Ventas por producto y categoría, incluyendo unidades vendidas e ingresos generados por SKU.

* Cuentas por cobrar: facturas pendientes, saldo, fecha de vencimiento y días de mora.

* Historial de ventas por rango de fechas, cliente, usuario y estado.

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

**5\. Módulo de Inventario: Cristobal**

**Pantallas sugeridas**

1. **Catálogo de Productos:** CRUD central de SKU, nombre, categoría, unidad de medida, precio de venta, costo base, stock mínimo y punto de reorden.

2. **Kardex:** trazabilidad de entradas, salidas, ajustes, documento de origen, usuario, fecha y saldo resultante.

3. **Control de Stock:** consulta de existencias actuales por producto y ubicación.

4. **Cálculo del Costo Promedio Ponderado:** actualización del CPP cuando se registran entradas valorizadas.

5. **Ajustes Manuales:** registro de inventario inicial, mermas, pérdidas, daños y correcciones autorizadas.

6. **Alertas de Reorden:** listado de productos cuyo stock está bajo el punto de reorden.

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

* Rotación de inventario y productos sin movimiento.

* Reporte de mermas, pérdidas y daños.

* Alertas de stock mínimo y punto de reorden.

**API sugerida**

* GET /api/productos

* POST /api/productos

* PUT /api/productos/{id}

* GET /api/inventario/stock

* GET /api/inventario/kardex/{productoId}

* POST /api/inventario/ajustes

* GET /api/inventario/alertas-reorden

* GET /api/reportes/valorizacion-inventario

**6\. Módulo de Contabilidad: Cristobal**

**Pantallas sugeridas**

1. **Plan de Cuentas:** estructura jerárquica de cuentas contables con código, nombre, tipo, naturaleza y estado.

2. **Libro Diario:** consulta y registro de asientos manuales y automáticos generados por Compras, Ventas e Inventario.

3. **Libro Mayor:** movimientos agrupados por cuenta y período, con débitos, créditos y saldo.

4. **Balance de Comprobación:** vista de saldos para comprobar el cuadre contable.

5. **Estado de Resultados:** ingresos por ventas menos costo de ventas y gastos operativos.

6. **Configuración de Integración:** asociación entre eventos del ERP y cuentas contables.

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

* GET /api/cuentas-contables

* POST /api/cuentas-contables

* GET /api/asientos

* POST /api/asientos

* GET /api/libro-mayor

* GET /api/reportes/balance-comprobacion

* GET /api/reportes/estado-resultados

* GET /api/reportes/libro-mayor

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

**Administración de usuarios y perfiles**

* Registro y edición de usuarios.

* Contraseñas almacenadas de forma segura mediante Identity.

* Asociación de usuarios a roles.

* Activación y desactivación de cuentas.

* Registro del último acceso.

* Recuperación y cambio de contraseña.

**RBAC: roles y permisos**

El control de acceso debe aplicarse tanto en la interfaz como en los endpoints del backend. Ocultar una opción del menú no reemplaza la autorización del servidor.

| Rol | Compras | Ventas | Inventario | Contabilidad | Administración |
| :---- | :---- | :---- | :---- | :---- | :---- |
| Administrador | Total | Total | Total | Total | Total |
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

* **ERP.Infrastructure:** Entity Framework Core, PostgreSQL, Identity y servicios externos.

* **ERP.Api:** controladores, autenticación, autorización, filtros y configuración HTTP.

* [**ERP.Web**](http://ERP.Web)**:** interfaz web, si el equipo utiliza Blazor, Razor Pages u otro frontend conectado a la API.

**11\. Plan de trabajo por etapas**

**Etapa 1: Base común**

* Crear la solución .NET y los proyectos por capas.

* Configurar GitHub y las ramas de trabajo.

* Configurar Supabase y la conexión PostgreSQL.

* Implementar Identity, JWT y roles iniciales.

* Definir entidades compartidas y migraciones.

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

* El backend [ASP.NET](http://ASP.NET) Core pueda conectarse correctamente a PostgreSQL en Supabase.

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

La adaptación a tres personas concentra en Cristobal las funciones que requieren mayor coordinación transversal: Inventario, Contabilidad, Integración y RBAC. Adan se enfoca en el ciclo de abastecimiento y Maria en el ciclo comercial. Con un backend centralizado en [ASP.NET](http://ASP.NET) Core, PostgreSQL en Supabase, un repositorio único en GitHub y despliegue accesible por Internet, el equipo puede desarrollar en paralelo y mantener todos los módulos conectados en una única solución ERP.