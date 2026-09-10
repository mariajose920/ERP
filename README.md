# ERP Integrado - .NET Core & Supabase PostgreSQL

ERP web centralizado para un equipo de 3 personas (**Adan**, **Maria**, **Cristobal**), desarrollado con **ASP.NET Core Web API**, **Entity Framework Core** y **PostgreSQL en Supabase**.

---

## 🏛️ Arquitectura de la Solución (Clean Architecture)

```
ERP/
├── .env                                # Variables de entorno y credenciales Supabase
├── ERPIntegrado.slnx                   # Solución .NET
├── src/
│   ├── ERP.Domain/                     # Entidades, Enums y Modelos de Dominio
│   ├── ERP.Application/                # DTOs, Casos de Uso e Interfaces de Servicio
│   ├── ERP.Infrastructure/             # EF Core, PostgreSQL Supabase, JWT y Servicios
│   └── ERP.Api/                        # Controladores REST, Swagger, Autenticación y RBAC
└── arquitectura_erp_3_personas_dotnet.md
```

---

## 👥 Distribución por Integrante

| Integrante | Módulo Principal | Responsabilidades |
| :--- | :--- | :--- |
| **Cristobal** | **Inventario, Contabilidad, RBAC e Integración** | Catálogo de productos, Kardex, Costo Promedio Ponderado (CPP), Ajustes, Plan de Cuentas, Libro Diario, Libro Mayor, Balance de Comprobación, Estado de Resultados, Autenticación JWT y Roles. |
| **Adan** | **Compras** | Proveedores, Órdenes de Compra, Recepción de Mercadería, Facturas de Compra, actualización de stock/kardex y generación automática de asientos contables. |
| **Maria** | **Ventas** | Clientes, Facturación con validación de existencias, Historial de Ventas, Pagos, deducción de stock y generación de asientos de Venta y Costo de Venta. |

---

## 🚀 Conexión con Supabase

- **Host (Connection Pooler):** `aws-0-us-east-2.pooler.supabase.com`
- **Puerto:** `5432`
- **Usuario:** `postgres.frwijnhngknsktcrxdfp`
- **Base de Datos:** `postgres`
- **Proyecto Supabase URL:** `https://frwijnhngknsktcrxdfp.supabase.co`

### Configuración en [.env](file:///c:/Users/mjvil/OneDrive/Escritorio/ERP_Osvaldo/ERP/.env) / [appsettings.json](file:///c:/Users/mjvil/OneDrive/Escritorio/ERP_Osvaldo/ERP/src/ERP.Api/appsettings.json)
```env
NEXT_PUBLIC_SUPABASE_URL=https://frwijnhngknsktcrxdfp.supabase.co
NEXT_PUBLIC_SUPABASE_PUBLISHABLE_KEY=sb_publishable__3F9JvqogfcJ-b_VHhmo-w_PWT34o2s
DATABASE_URL=postgresql://postgres.frwijnhngknsktcrxdfp:ProyectoERP@aws-0-us-east-2.pooler.supabase.com:5432/postgres
```

---

## ⚡ Cómo ejecutar la API

1. **Compilar la solución:**
   ```bash
   dotnet build ERPIntegrado.slnx
   ```

2. **Ejecutar la API:**
   ```bash
   dotnet run --project src/ERP.Api/ERP.Api.csproj
   ```

3. **Abrir Swagger UI:**
   Navega en tu navegador a:
   👉 `http://localhost:5195`

---

## 🔐 Credenciales Iniciales

- **Usuario Administrador:** `admin@erp.com`
- **Contraseña:** `Admin123!`
- **Roles predefinidos:** `Administrador`, `Comprador`, `Vendedor`, `Bodeguero`, `Contador`.

---

## 📡 Endpoints Principales

### 🔑 Autenticación & Seguridad
- `POST /api/Auth/login`: Iniciar sesión y obtener token JWT.
- `POST /api/Auth/register`: Crear nuevos usuarios con rol.

### 🏢 Maestros
- `GET /api/Maestros/productos` | `POST /api/Maestros/productos`
- `GET /api/Maestros/categorias` | `POST /api/Maestros/categorias`
- `GET /api/Maestros/proveedores` | `POST /api/Maestros/proveedores`
- `GET /api/Maestros/clientes` | `POST /api/Maestros/clientes`
- `GET /api/Maestros/cuentas-contables` | `POST /api/Maestros/cuentas-contables`

### 🛒 Compras (Adan)
- `GET /api/Compras/ordenes` | `POST /api/Compras/ordenes`
- `POST /api/Compras/recepciones`
- `GET /api/Compras/facturas` | `POST /api/Compras/facturas`

### 🏷️ Ventas (Maria)
- `GET /api/Ventas` | `POST /api/Ventas`
- `POST /api/Ventas/{id}/anular`
- `POST /api/Ventas/pagos`

### 📦 Inventario (Cristobal)
- `GET /api/Inventario/stock`
- `GET /api/Inventario/kardex/{productoId}`
- `POST /api/Inventario/ajustes`
- `GET /api/Inventario/alertas-reorden`
- `GET /api/Inventario/valorizacion`

### 📒 Contabilidad (Cristobal)
- `POST /api/Contabilidad/asientos`
- `GET /api/Contabilidad/libro-diario`
- `GET /api/Contabilidad/libro-mayor/{cuentaId}`
- `GET /api/Contabilidad/balance-comprobacion`
- `GET /api/Contabilidad/estado-resultados`

### 📊 Reportes
- `GET /api/Reportes/compras-por-proveedor`
- `GET /api/Reportes/ventas-por-cliente`
- `GET /api/Reportes/cuentas-por-cobrar`
