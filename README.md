# ERP Integrado - .NET Core & SQL Server SQL Server

ERP web centralizado para un equipo de 3 personas (**Adan**, **Maria**, **Cristobal**), desarrollado con **ASP.NET Core Web API**, **Entity Framework Core** y **SQL Server en SQL Server**.

---

## 🏛️ Arquitectura de la Solución (Clean Architecture)

```
ERP/
├── .env                                # Variables de entorno y credenciales SQL Server
├── ERPIntegrado.slnx                   # Solución .NET
├── src/
│   ├── ERP.Domain/                     # Entidades, Enums y Modelos de Dominio
│   ├── ERP.Application/                # DTOs, Casos de Uso e Interfaces de Servicio
│   ├── ERP.Infrastructure/             # EF Core, SQL Server SQL Server, JWT y Servicios
│   └── ERP.Api/                        # Controladores REST, Swagger, Autenticación y RBAC
└── arquitectura_erp_3_personas_dotnet.md
```

---

## 👥 Distribución por Integrante

| Integrante | Módulo Principal | Responsabilidades |
| :--- | :--- | :--- |
| **Cristóbal** | **Contabilidad y Administración** | Autenticación de usuarios JWT, Autorización por roles (RBAC), Auditoría de eventos, Plan de Cuentas, Centralización contable (Libro Diario, Libro Mayor), Asientos de reversa y Estados Financieros. |
| **Adán** | **Compras e Inventario** | Catálogo de productos, Control de Kardex inmutable, Ajustes, Proveedores, Órdenes de Compra, Recepción de Mercadería y actualización de stock. |
| **María** | **Ventas** | Mantenedor de Clientes, Interfaz POS, Historial de Ventas, Cobranzas a plazo (abonos en cascada) y Reportes analíticos. |

---

## 🚀 Conexión con SQL Server

- **Host (Connection Pooler):** `aws-0-us-east-2.pooler.SQL Server.com`
- **Puerto:** `5432`
- **Usuario:** `sqlserver.frwijnhngknsktcrxdfp`
- **Base de Datos:** `sqlserver`
- **Proyecto SQL Server URL:** `https://frwijnhngknsktcrxdfp.SQL Server.co`

### Configuración en [.env](file:///c:/Users/mjvil/OneDrive/Escritorio/ERP_Osvaldo/ERP/.env) / [appsettings.json](file:///c:/Users/mjvil/OneDrive/Escritorio/ERP_Osvaldo/ERP/src/ERP.Api/appsettings.json)
```env
NEXT_PUBLIC_SQL Server_URL=https://frwijnhngknsktcrxdfp.SQL Server.co
NEXT_PUBLIC_SQL Server_PUBLISHABLE_KEY=sb_publishable__3F9JvqogfcJ-b_VHhmo-w_PWT34o2s
DATABASE_URL=SQL Server://sqlserver.frwijnhngknsktcrxdfp:ProyectoERP@aws-0-us-east-2.pooler.SQL Server.com:5432/sqlserver
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

### 📒 Contabilidad y Seguridad (Cristóbal)
- `POST /api/Auth/login`: Iniciar sesión y obtener token JWT.
- `POST /api/Auth/register`: Crear nuevos usuarios con rol.
- `GET /api/Maestros/cuentas-contables` | `POST /api/Maestros/cuentas-contables`
- `POST /api/Contabilidad/asientos`
- `GET /api/Contabilidad/libro-diario`
- `GET /api/Contabilidad/libro-mayor/{cuentaId}`
- `GET /api/Contabilidad/balance-comprobacion`
- `GET /api/Contabilidad/estado-resultados`

### 🛒 Compras e Inventario (Adán)
- `GET /api/Maestros/productos` | `POST /api/Maestros/productos`
- `GET /api/Maestros/categorias` | `POST /api/Maestros/categorias`
- `GET /api/Maestros/proveedores` | `POST /api/Maestros/proveedores`
- `GET /api/Compras/ordenes` | `POST /api/Compras/ordenes`
- `POST /api/Compras/recepciones`
- `GET /api/Inventario/stock`
- `GET /api/Inventario/kardex/{productoId}`
- `POST /api/Inventario/ajustes`
- `GET /api/Inventario/valorizacion`

### 🏷️ Ventas (María)
- `GET /api/Maestros/clientes` | `POST /api/Maestros/clientes`
- `GET /api/Ventas` | `POST /api/Ventas`
- `POST /api/Ventas/{id}/anular`
- `POST /api/Ventas/pagos`
- `GET /api/Reportes/ventas-por-cliente`
