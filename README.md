# ERP Integrado - .NET y SQL Server

ERP web centralizado para un equipo de 3 personas (**Adán**, **María**, **Cristóbal**), desarrollado con **ASP.NET Core Web API (.NET 10)**, **Entity Framework Core** y **SQL Server**. El frontend (HTML + React/Babel/Tailwind por CDN) se sirve desde la misma API.

---

## 🏛️ Arquitectura de la Solución (Clean Architecture)

```
ERP/
├── .env                      # Variables locales (NO se versiona): cadena de conexión y clave JWT
├── ERPIntegrado.slnx         # Solución .NET
├── src/
│   ├── ERP.Domain/           # Entidades y enums (un archivo por módulo/responsable)
│   ├── ERP.Application/      # DTOs/<Módulo>, Interfaces (una por archivo), Common (constantes, JwtSettings)
│   ├── ERP.Infrastructure/   # EF Core (SQL Server), servicios, numerador, auditoría, migraciones
│   └── ERP.Api/              # Controladores REST, Swagger, JWT/RBAC, manejo global de errores
│       └── wwwroot/          # FRONTEND (fuente única). shared/: erp-common.js (sesión, permisos, apiFetch),
│                             #   navbar.js (menú por rol), erp-ui.css (diseño) y erp-ui.jsx (componentes React)
├── Frontend/                 # OBSOLETA (ver Frontend/README.md)
├── tools/                    # Scripts de mantenimiento (node / python)
└── arquitectura_erp_3_personas_dotnet.md
```

---

## 👥 Distribución por Integrante

| Integrante | Módulo Principal | Responsabilidades |
| :--- | :--- | :--- |
| **Adán** | **Compras** | Proveedores, órdenes de compra (con aprobación), recepción de mercadería, facturas de compra y su asiento, historial de compras y reportes de abastecimiento. |
| **María** | **Ventas** | Mantenedor de clientes, POS, historial de ventas, anulaciones, cobranzas a plazo (abonos en cascada), caja diaria y reportes de ventas (incluye comisiones sugeridas). |
| **Cristóbal** | **Inventario, Contabilidad, Seguridad e Integración** | Catálogo de productos, kardex, CPP, ajustes, alertas y valorización de inventario; autenticación JWT, RBAC, auditoría; plan de cuentas, asientos, Libro Diario/Mayor, estados financieros e integración entre módulos. |

> La división de módulos es la de `arquitectura_erp_3_personas_dotnet.md` y la del documento del Trabajo Final
> (`Trabajo Taller de Ing en Software.md`): **Inventario pertenece a Cristóbal**.

### 📋 Pantallas y reportes por integrante (mínimo 5 + 5 cada uno)

| Integrante | Pantallas (5) | Reportes (5) |
| :--- | :--- | :--- |
| **María** | `pantalla_caja_inicio` · `pantalla_emision_ventas` (POS) · `pantalla_gestion_clientes` · `pantalla_cobranzas_plazo` · `pantalla_historial_directas` | En `pantalla_reportes_ventas` y `pantalla_cobranzas_ventas`: resumen de ventas · ventas por cliente · ventas por producto · cuentas por cobrar · comisiones sugeridas (ventas por día es un extra) |
| **Adán** | `pantalla_gestion_proveedores` · `pantalla_ordenes_compra` · `pantalla_recepcion_mercaderia` · `pantalla_facturas_compra` · `pantalla_historial_compras` | En `pantalla_reportes_compras`: compras por proveedor · historial de precios · cumplimiento de OC · consolidado de facturas · cuentas por pagar |
| **Cristóbal** | `pantalla_catalogo_productos` · `pantalla_ajustes_alertas_inventario` · `pantalla_plan_cuentas` · `pantalla_libro_diario` · `pantalla_admin_usuarios` | `pantalla_kardex_inventario` · `pantalla_valorizacion_inventario` · `pantalla_libro_mayor` · `pantalla_reportes_contabilidad` (balance de comprobación) · `pantalla_estados_financieros` (estado de resultados) |

Extras de Cristóbal: `pantalla_auditoria`, `pantalla_configuracion_integracion` y el balance general. Los 30 requerimientos
funcionales (10 por integrante) están en el capítulo 6.3.1 del documento del Trabajo Final.

Cada archivo de código indica en su primera línea el módulo y el responsable
(`// Módulo: Ventas — Responsable: María` o `<!-- Módulo: ... — Responsable: ... -->`).

---

## 🔐 Configuración y credenciales

**Nunca** publique credenciales en este README ni en `appsettings.json`.

- `appsettings.json`: solo valores no sensibles. `Jwt:SecretKey` va vacío.
- `appsettings.Development.json`: clave JWT y contraseña de seed **solo para desarrollo local**.
- `.env` (raíz del repo, ignorado por git) o *user-secrets* para valores reales:

```env
ConnectionStrings__DefaultConnection=Server=<SERVIDOR>;Database=<BASE_DE_DATOS>;User Id=<USUARIO>;Password=<CONTRASEÑA>;TrustServerCertificate=True;
Jwt__SecretKey=<CLAVE_ALEATORIA_DE_AL_MENOS_32_CARACTERES>
Jwt__Issuer=ERPIntegrado
Jwt__Audience=ERPIntegradoClient
Seed__AdminPassword=<CONTRASEÑA_INICIAL_DEL_ADMIN>
Seed__CajeroPassword=<CONTRASEÑA_DE_cajero@erp.com_SI_NO_EXISTE_CAJA1>
```

Alternativa con user-secrets:

```bash
dotnet user-secrets init --project src/ERP.Api
dotnet user-secrets set "Jwt:SecretKey" "<CLAVE>" --project src/ERP.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<CADENA>" --project src/ERP.Api
```

> ⚠️ Versiones anteriores de este README publicaron host, usuario, contraseña y claves reales.
> Esas credenciales deben considerarse comprometidas y **rotarse** (tarea del equipo).

SQL Server usa por defecto el puerto **1433** (el 5432 que figuraba antes es de PostgreSQL).

---

## ⚡ Cómo ejecutar la API

1. Compilar: `dotnet build ERPIntegrado.slnx`
2. Ejecutar: `dotnet run --project src/ERP.Api/ERP.Api.csproj`
3. Frontend: `http://localhost:5195` (redirige a `login.html` si no hay sesión).
4. Swagger (solo en Development): `http://localhost:5195/swagger` → botón **Authorize** con el token.

### Migraciones

El esquema se gestiona con migraciones de EF Core (`Database.Migrate()` al iniciar).

- Crear una migración: `dotnet ef migrations add <Nombre> --project src/ERP.Infrastructure --startup-project src/ERP.Api`
- Si la base fue creada antes con `EnsureCreated()` (no tiene la tabla `__EFMigrationsHistory`), la API **no**
  aplica migraciones y lo advierte en el log. Para regularizarla, registre la migración inicial como aplicada:

```sql
CREATE TABLE [__EFMigrationsHistory] ([MigrationId] nvarchar(150) NOT NULL PRIMARY KEY, [ProductVersion] nvarchar(32) NOT NULL);
INSERT INTO [__EFMigrationsHistory] VALUES ('20260928123828_InitialCreate', '10.0.11');
```

---

## 🔑 Autenticación y perfiles

- Todos los endpoints exigen JWT, excepto `POST /api/auth/login` y `GET /api/auth/demo-credenciales` (este último solo responde en Development).
- `login.html` llama a la API, guarda token, rol y permisos (`auth_token`, `auth_usuario`) y redirige según el rol:
  **Administrador** y perfiles de gestión → `index_erp.html` (portal); **Cajero** → `pantalla_caja_inicio.html`.
- El menú (`shared/navbar.js`) solo muestra las pantallas permitidas. Si un perfil abre una URL no permitida, se le envía a
  `acceso_denegado.html`. Al expirar el token (12 h por defecto) la sesión se cierra y se vuelve al login. Botón **Cerrar sesión** en la barra.
- Contraseñas con PBKDF2; los hashes SHA-256 antiguos se migran solos en el primer login.
- Cada permiso es una política de autorización; el rol Administrador cumple todas.

### 🧪 Credenciales de demostración (solo desarrollo)

Se muestran también en un recuadro de `login.html` cuando la API corre en Development.

| Perfil | Usuario | Contraseña | Origen |
| :--- | :--- | :--- | :--- |
| Administrador | `admin@erp.com` | `Admin123!` | `Seed:AdminPassword` en `appsettings.Development.json` |
| Cajero | `CAJA1` | `CAJA123` | Usuario del seed reasignado al rol Cajero |
| Cajero (solo si no existe CAJA1) | `cajero@erp.com` | `Caja123!` | `Seed:CajeroPassword` en `appsettings.Development.json` |

> En producción no defina `Seed:*` en appsettings: use variables de entorno y cambie las contraseñas tras el primer ingreso.

### 🧾 Perfil Cajero (permisos exactos)

| Permiso | Permite |
| :--- | :--- |
| `ventas.documentos.crear` | Emitir ventas en el POS |
| `ventas.productos.consultar` | Ver productos, precios y stock en caja (`GET /api/ventas/catalogo`, sin costos) |
| `ventas.clientes.leer` | Buscar clientes |
| `ventas.clientes.crear` | Crear clientes desde caja (con límite de crédito) |
| `ventas.clientes.escribir` | Editar datos del cliente y su límite de crédito desde el POS (también habilita dar de baja clientes sin deuda) |
| `ventas.pagos.crear` | Ver deudas pendientes y registrar abonos (individuales o en cascada) |
| `ventas.caja.propias` | Ver sus propias ventas y abonos del día (`GET /api/ventas/caja/hoy`) |

Sin acceso a compras, inventario (ajustes, kardex), contabilidad, reportes ni administración. El seed restablece
exactamente estos permisos en cada arranque y la API no permite editarlos desde la pantalla de roles.

---

## 📡 Endpoints Principales

### 📒 Contabilidad, Seguridad e Integración (Cristóbal)
- `POST /api/auth/login` · `POST /api/auth/register` · `GET /api/auth/demo-credenciales` (Development)
- `GET /api/usuarios` · `PUT /api/usuarios/{id}` · `GET /api/roles` · `PUT /api/roles/{id}/permisos` · `GET /api/permisos`
- `GET|POST /api/maestros/cuentas-contables` · `PUT /api/maestros/cuentas-contables/{id}`
- `POST /api/contabilidad/asientos` · `GET /api/contabilidad/libro-diario` · `GET /api/contabilidad/libro-mayor/{cuentaId}`
- `GET /api/contabilidad/balance-comprobacion` · `GET /api/contabilidad/estado-resultados` · `GET /api/contabilidad/balance-general`
- `GET /api/dashboard/resumen` · `GET /api/integracion/configuracion` · `GET /api/integracion/estado` · `GET /api/integracion/auditoria`
- `/api/store` **obsoleto** (el puente localStorage → SQL se retiró; la tabla `ErpKeyValue` se conserva)

### 🛒 Compras (Adán)
- `GET|POST /api/proveedores` · `GET|PUT|DELETE /api/proveedores/{id}` (también `/api/maestros/proveedores`)
- `GET|POST /api/compras/ordenes` · `POST /api/compras/ordenes/{id}/aprobar` · `POST /api/compras/ordenes/{id}/anular`
  (flujo: Pendiente → Aprobada → recepción → factura → Liquidada; **solo una OC aprobada admite recepciones**)
- `GET|POST /api/compras/recepciones` · `GET|POST /api/compras/facturas`
- Reportes: `GET /api/reportes/compras-por-proveedor` · `historial-precios-compra?productoId=` · `cumplimiento-ordenes` · `consolidado-facturas-compra?estado=&proveedorId=` · `cuentas-por-pagar`
  (la API no registra pagos a proveedores: el saldo por pagar de cada factura aprobada es su total)

### 📦 Inventario (Cristóbal)
- `GET|POST /api/maestros/productos` · `PUT|DELETE /api/maestros/productos/{id}` · `GET|POST /api/maestros/categorias`
- `GET /api/inventario/stock` · `GET /api/inventario/kardex/{productoId}?desde=&hasta=&pagina=&tamanoPagina=`
- `GET|POST /api/inventario/ajustes` · `GET /api/inventario/alertas-reorden` · `GET /api/inventario/valorizacion`
- `GET /api/integracion/auditoria?desde=&hasta=&usuario=&modulo=&accion=&entidad=&top=` (filtros de la pantalla de auditoría)

### 🏷️ Ventas (María)
- `GET|POST /api/clientes` · `GET|PUT|DELETE /api/clientes/{id}` (también `/api/maestros/clientes`)
- `GET|POST /api/ventas` · `POST /api/ventas/{id}/anular` (cuerpo `{ "motivo": "..." }`)
- `GET /api/ventas/catalogo` · `GET /api/ventas/pendientes?clienteId=` · `POST /api/ventas/pagos` · `POST /api/ventas/pagos/cascada` · `GET /api/ventas/caja/hoy`
- `GET /api/reportes/resumen-ventas` · `ventas-por-dia` · `ventas-por-cliente` · `ventas-por-producto` · `cuentas-por-cobrar` · `comisiones-ventas?porcentaje=2`

**IVA y clientes en ventas:** el precio de venta de cada producto **incluye IVA (19%)**. El cliente paga exactamente la suma de los precios (menos descuentos); el neto y el IVA se calculan hacia atrás (neto = total / 1,19) y se muestran como detalle, sin sumarse. El cliente es opcional: sin cliente la venta se asocia a `CONSUMIDOR FINAL` (RUT 66.666.666-6), solo al contado.

Métodos de pago válidos: `Efectivo`, `Transferencia`, `Tarjeta` (pagadas al emitir) y `Credito` (30 días, valida límite de crédito).
Los enums se devuelven como texto (`"RecibidaParcial"`, `"Anulada"`...).

---

## 🎨 Frontend

- Cada pantalla carga `shared/erp-common.js` (antes que nada), Tailwind, `shared/erp-ui.css`, React 18.3.1 y Babel, y luego
  `shared/erp-ui.jsx` (componentes: `Pagina`, `Kpi`, `Tabla`, `Modal`, `confirmar`, `Badge*`, `SemaforoMora`, `Progreso`...).
- Paleta por módulo (automática según la pantalla): Ventas verde, Compras ámbar, Inventario celeste, Contabilidad violeta, Administración rosa.
- Ninguna pantalla guarda datos de negocio en `localStorage`; solo la sesión.
