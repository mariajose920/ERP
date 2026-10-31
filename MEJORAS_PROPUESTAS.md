# Mejoras propuestas – ERP Integrado

Análisis del backend (.NET 10 / EF Core / SQL Server) y del frontend (pantallas HTML con React + Babel vía CDN; 28 tras la ronda 3).
Las mejoras se agrupan según la división de módulos del README:
**Adán** (Compras e Inventario en las rondas 1-2; desde la ronda 3 solo Compras), **María** (Ventas), **Cristóbal** (Contabilidad, Seguridad/RBAC e integración; desde la ronda 3 también Inventario).

---

## 0. Transversales (las ejecuta Cristóbal como responsable de integración)

### Código
- **T1 – Seguridad de la API:** ningún controlador tiene `[Authorize]` (solo `register`). Agregar `[Authorize]` a nivel de controlador y `[AllowAnonymous]` solo en `login`.
- **T2 – Login falso en el frontend:** `login.html` compara contra `admin@erp.com / Admin123!` escrito en el JS y guarda `isAuthenticated` en localStorage. Debe llamar a `POST /api/auth/login`, guardar el JWT y enviarlo en `Authorization: Bearer` (helper común `apiFetch`).
- **T3 – Credenciales expuestas:** el README publica host, usuario, contraseña y clave de la BD; `appsettings.json` contiene la clave JWT. Quitarlas del README (dejar placeholders) y documentar el uso de `.env` / user-secrets. *(Rotar las claves reales queda a cargo del equipo.)*
- **T4 – `Program.cs`:** `if (app.Environment.IsDevelopment() || true)` fuerza Swagger en producción; CORS `AllowAll` se registra después de `UseStaticFiles`; `EnsureCreated()` convive con migraciones (usar `Migrate()`); claves JWT por defecto duplicadas y distintas entre `Program.cs` y `AuthService`. Centralizar en una clase `JwtSettings`.
- **T5 – Manejo de errores:** cada controlador repite `try/catch InvalidOperationException`. Reemplazar por un middleware/`IExceptionHandler` global que devuelva `ProblemDetails`.
- **T6 – Numeración de documentos:** `CountAsync() + 1` genera números duplicados con concurrencia y al anular. Crear un servicio `INumeradorDocumentos` (tabla de secuencias o `MAX` dentro de la transacción) usado por todos los módulos.
- **T7 – Constantes contables:** los códigos `"1.1.03.01"`, `"2.1.01.01"`, etc. y el IVA `0.19m` están repetidos en 3 servicios. Moverlos a una clase `CuentasContablesConst` / `ImpuestosConst`.
- **T8 – Duplicación Frontend ↔ wwwroot:** existen dos copias de las pantallas. Dejar `src/ERP.Api/wwwroot` como única fuente y documentarlo (o copiar en el build).
- **T9 – Scripts sueltos:** `check.py`, `script.py`, `fix_everything.py` tienen rutas absolutas del PC personal. Moverlos a `tools/` con rutas relativas o eliminarlos.
- **T10 – README:** corregir "SQL Server en SQL Server", puerto 5432 (es de PostgreSQL), y la tabla de responsables que contradice `arquitectura_erp_3_personas_dotnet.md`.

### Visual (común a todas las pantallas)
- **V1 – Navbar duplicada en 21 archivos** (sincronizada con scripts Python). Extraer a `shared/navbar.js` que la inyecte.
- **V2 – CDN de desarrollo:** `react.development.js`, Babel en el navegador y `cdn.tailwindcss.com` sin versión. Fijar versiones exactas y usar builds `production.min`.
- **V3 – `alert()`** como única retroalimentación (≈25 usos). Crear un componente de notificación (toast) común.
- **V4 – Líneas en blanco entre cada línea** en varios HTML (artefacto de conversión de saltos de línea). Normalizar formato.
- **V5 – Formato de moneda/fechas:** unificar con helpers `formatCLP()` / `formatFecha()` (`Intl.NumberFormat('es-CL')`).
- **V6 – Accesibilidad:** `label for` en todos los inputs, `aria-label` en botones de solo ícono, contraste en textos `text-xs text-indigo-200`.

### Comentarios
- **C1 – Idioma mixto:** comentarios en inglés (`// Update order status`, `// Recalculate CPP`) y español. Unificar en español.
- **C2 – XML docs (`/// <summary>`)** en todas las interfaces de servicio, DTOs y endpoints públicos (habilita la doc de Swagger).
- **C3 – Comentarios desactualizados:** `// 1. Connection String to SQL Server Local`, el comentario de VentasService que dice "Clientes (2.1.01.01)" cuando el código usa `1.1.02.01`.

---

## 1. Adán – Compras e Inventario

### Código
- **A1 – `ComprasController`:** sin `try/catch` ni validación → una recepción inválida devuelve 500. Validar DTOs (`[Required]`, `Items` no vacío, cantidades > 0).
- **A2 – Falta CRUD de proveedores** (`PUT`/`DELETE` lógicos) que pide la arquitectura; hoy solo existen GET/POST en `MaestrosController`. Crear `ProveedoresController`.
- **A3 – Factura de compra:** no valida que el proveedor/orden existan ni que las cantidades facturadas coincidan con lo recibido; aumenta stock aunque la mercadería ya se haya recibido (doble conteo conceptual). Documentar/decidir que el stock entra en la factura y validar contra la recepción.
- **A4 – CPP con stock negativo/cero:** revisar la fórmula en `RegistrarYAprobarFacturaCompraAsync` y extraerla a un método `CalcularCpp` testeable en `InventarioService`.
- **A5 – Saldo de Proveedores:** `ctaProveedores.SaldoActual += total` ignora la naturaleza de la cuenta (inconsistente con `ContabilidadService`). Usar un método común de actualización de saldo.
- **A6 – Ajustes de inventario:** `AsientoContableId` nunca se genera; el ajuste no impacta contabilidad. Generar asiento (Merma/Pérdida vs Inventario). `UsuarioEmail` del DTO no se usa → eliminarlo.
- **A7 – Kardex:** `GetKardexPorProductoAsync` sin paginación ni filtro de fechas.
- **A8 – Pantallas Compras/Inventario** (`ordenes_compra`, `recepcion_facturas_compra`, `catalogo_productos`, `kardex_inventario`, `ajustes_alertas_inventario`) usan localStorage en vez de la API existente (`/api/compras/*`, `/api/inventario/*`). Conectarlas.

### Visual
- Estados de OC con badges de color consistentes (Pendiente, Parcial, Total, Liquidada, Anulada).
- Barra de progreso de recepción (recibido/pedido) por línea de orden.
- Resaltar productos bajo punto de reorden en catálogo y kardex.

### Comentarios
- Documentar la regla de negocio del CPP y del asiento de compra (Debe Inventario + IVA CF / Haber Proveedores) en `/// <summary>`.

---

## 2. María – Ventas

### Código
- **M1 – Bug Kardex "PENDIENTE":** `EmitirVentaAsync` actualiza todos los movimientos con `DocumentoNumero == "PENDIENTE"` del usuario, no solo los de esta venta. Guardar la venta primero y asignar el número directamente a los movimientos creados.
- **M2 – Anulación incompleta:** `AnularVentaAsync` repone stock pero no genera asientos de reversa (venta y costo) como exige la arquitectura.
- **M3 – Límite de crédito:** no se valida `Cliente.LimiteCredito` en ventas a crédito.
- **M4 – `FechaVencimiento`** nunca se asigna → `DiasMora` en reportes siempre 0. Asignar según condición de pago.
- **M5 – Método de pago como string mágico** (`"Efectivo"` comparado 4 veces). Usar enum `MetodoPago`.
- **M6 – `AnularVenta([FromBody] string motivo)`** → usar DTO `AnularVentaDto`.
- **M7 – Validar** `Descuento <= Cantidad*Precio`, cantidades > 0, cliente activo.
- **M8 – CRUD de clientes** vive en `MaestrosController` recibiendo la entidad directamente (overposting de `SaldoPendiente`). Crear `ClientesController` con DTOs.
- **M9 – Pantallas** `emision_ventas`, `historial_directas`, `cobranzas_plazo`, `cobranzas_ventas`, `reportes_ventas` usan localStorage (hasta 22 accesos). Conectarlas a `/api/ventas` y `/api/reportes`.

### Visual
- POS: mostrar stock disponible por producto y bloquear el botón cuando la cantidad lo excede.
- Indicador visual de mora (verde/amarillo/rojo) en cobranzas.
- Confirmación modal antes de anular una venta (no `confirm()` nativo).

### Comentarios
- Corregir el comentario de cuentas del asiento de venta y documentar el flujo de abono en cascada.

---

## 3. Cristóbal – Contabilidad, Seguridad e Integración

### Código
- **K1 – Asiento manual:** no valida que haya ≥2 líneas, que cada línea tenga solo Debe o Haber, ni que la cuenta exista y `PermiteMovimiento`.
- **K2 – Balance de comprobación:** carga **todos** los detalles en memoria y filtra por cuenta en un bucle (O(n·m)). Agrupar en SQL (`GroupBy` en la consulta).
- **K3 – Inconsistencia de saldos:** `SaldoActual` se actualiza distinto en cada servicio. Centralizar en `ContabilidadService.AplicarMovimiento(cuenta, debe, haber)` y que Compras/Ventas lo usen (o calcular saldo desde detalles).
- **K4 – Auditoría:** la entidad `Auditoria` existe pero nunca se escribe. Implementar un interceptor de `SaveChanges` o un `IAuditoriaService`.
- **K5 – RBAC:** los permisos se emiten en el JWT pero no se usan. Crear políticas (`[Authorize(Policy = "compras.ordenes.crear")]`) y aplicarlas por módulo.
- **K6 – `StoreController`:** sin autenticación, `DELETE` no valida prefijo `erp_`, tabla creada con SQL crudo fuera de migraciones. Protegerlo y planear su retiro cuando las pantallas usen la API real.
- **K7 – Hash de contraseñas:** revisar `DbInitializer.HashPassword` (usar PBKDF2/`PasswordHasher<T>`).
- **K8 – `RegisterUserAsync`:** la excepción de correo duplicado devuelve 500 (cubierto por T5).
- **K9 – Entidades devueltas directamente** por la API (ciclos, sobreexposición de `PasswordHash` vía navegación). Devolver DTOs.
- **K10 – Pantallas contables** (`libro_diario`, `libro_mayor`, `plan_cuentas`, `estados_financieros`, `reportes_contabilidad`, `admin_usuarios`) → conectar a `/api/contabilidad/*` y `/api/auth`.

### Visual
- Libro diario: indicador "cuadrado / descuadrado" por asiento y totales al pie.
- Plan de cuentas como árbol colapsable por nivel.
- Estados financieros imprimibles (`@media print`).

### Comentarios
- Documentar cada endpoint con `/// <summary>` y `<response>` para Swagger; documentar la convención de naturaleza deudora/acreedora.

---

## Prioridad sugerida
1. **Crítico:** T1, T2, T3, M1, T6 (seguridad y corrupción de datos).
2. **Alto:** T5, K3, M2, A3, A6, K1, K2.
3. **Medio:** conexión de pantallas a la API (A8, M9, K10), V1–V3.
4. **Bajo:** comentarios (C1–C3), estilo visual, accesibilidad.

---

## Estado de implementación

Ronda de octubre 2026. Orden de trabajo: Transversales → Adán → María → Cristóbal.
Respaldo previo de `src/` y `Frontend/` (sin `bin/` ni `obj/`): `ERP_backup_antes_mejoras/` (carpeta hermana de `ERP/`).
Compilación final: `dotnet build ERPIntegrado.slnx` → **0 advertencias, 0 errores**.
`dotnet ef migrations has-pending-model-changes` → sin cambios de modelo, **no se creó migración** (T6 y M5 se resolvieron sin tocar el esquema).
No se ejecutó la API contra la base de datos (no hay prueba de integración en tiempo de ejecución).

Rutas abreviadas: `Api/` = `src/ERP.Api/`, `App/` = `src/ERP.Application/`, `Infra/` = `src/ERP.Infrastructure/`, `Dom/` = `src/ERP.Domain/`, `www/` = `src/ERP.Api/wwwroot/`.

### 0. Transversales (Cristóbal)

| ID | Estado | Archivos | Notas |
| :-- | :-- | :-- | :-- |
| T1 | Hecho | `Api/Program.cs`, todos los `Api/Controllers/*` | `[Authorize]` en cada controlador + `FallbackPolicy` autenticada; `[AllowAnonymous]` solo en `login`. |
| T2 | Hecho | `www/login.html`, `www/shared/erp-common.js`, `www/erp-sync.js`, todas las pantallas | Login real contra `/api/auth/login`; token en `localStorage.auth_token` (sin prefijo `erp_`, no se sincroniza); `apiFetch` envía `Bearer`; guard de sesión en todas las pantallas. |
| T3 | Hecho | `README.md`, `Api/appsettings*.json` | README con placeholders; clave JWT fuera de `appsettings.json` (queda solo en `appsettings.Development.json` y `.env`). `.env` no se modificó. **Rotar las credenciales publicadas queda a cargo del equipo.** |
| T4 | Hecho | `Api/Program.cs`, `App/Common/JwtSettings.cs`, `Infra/Services/AuthService.cs` | Swagger solo en Development (con botón Authorize); CORS antes de estáticos y configurable; `Migrate()` en vez de `EnsureCreated()` (si la BD no tiene historial de migraciones solo advierte, ver README); JWT centralizado y validado al iniciar; `.env` ahora sí alimenta `IConfiguration`. |
| T5 | Hecho | `Api/Infraestructura/ManejadorExcepcionesGlobal.cs`, controladores | `IExceptionHandler` + `ProblemDetails` (404/400/403/409/500, con propiedad `error` por compatibilidad); se eliminaron los `try/catch`. |
| T6 | Hecho | `App/Interfaces/INumeradorDocumentos.cs`, `Infra/Services/Comun/NumeradorDocumentos.cs`, servicios | `MAX` dentro de la transacción + `sp_getapplock` exclusivo; sin tabla nueva ni migración. Todos los documentos (OC, RC, FV, AS, AJ) lo usan. |
| T7 | Hecho | `App/Common/CuentasContablesConst.cs` (incluye `ImpuestosConst`) | Sin literales de cuentas ni `0.19m` en los servicios. |
| T8 | Hecho | `www/*`, `Frontend/README.md` | Solo diferían `pantalla_libro_diario.html` y `pantalla_admin_usuarios.html`; se conservó la versión de `Frontend/` (más completa) y se copió a `wwwroot`. Detalle en `Frontend/README.md`. |
| T9 | Hecho | `tools/check.py`, `tools/script.py`, `tools/fix_everything.py` | Movidos con rutas relativas; `script.py` y `fix_everything.py` marcados obsoletos (la navbar ahora es `shared/navbar.js`). |
| T10 | Hecho | `README.md` | Corregido "SQL Server en SQL Server" y puerto (1433). La tabla sigue esta propuesta (Inventario → Adán); **`arquitectura_erp_3_personas_dotnet.md` aún asigna Inventario a Cristóbal: decisión pendiente del equipo.** |
| V1 | Hecho | `www/shared/navbar.js`, todas las pantallas, `tools/aplicar_layout_comun.js` | Navbar inyectada desde un solo archivo; muestra el usuario logueado. |
| V2 | Parcial | todas las pantallas | React 18.3.1 `production.min`, Babel 7.26.4 y Tailwind 3.4.16 con versión fija. Pendiente: precompilar JSX y Tailwind (requiere build con Node). |
| V3 | Hecho | `www/shared/erp-common.js` | `mostrarToast()` común; `window.alert` redirigido a toast (los ≈25 usos quedan no bloqueantes). `confirm()` nativo se mantiene. |
| V4 | Hecho | 18 HTML, `tools/normalizar_lineas.js` | Eliminadas las líneas en blanco intercaladas. |
| V5 | Hecho | `www/shared/erp-common.js`, 16 pantallas, `tools/unificar_formato_moneda.js` | `formatCLP`, `formatNumero`, `formatFecha`; las copias locales de `formatearMoneda` apuntan a `formatCLP`. |
| V6 | Hecho (ronda 2) | `www/login.html`, `www/shared/*` | `aria-label` en el botón de menú, `aria-current`, toasts con `aria-live`. Pendiente: revisar `label for` y contraste pantalla por pantalla. |
| C1 | Hecho | todos los `.cs` (excepto `Migrations/`, generadas) | Comentarios en español. |
| C2 | Hecho | `App/DTOs/*`, `App/Interfaces/*`, `Api/Controllers/*` | `/// <summary>` y `<response>`; XML de documentación habilitado en Api y Application e incluido en Swagger. |
| C3 | Hecho | `Api/Program.cs`, `Infra/Services/VentasService.cs` | Corregidos los comentarios desactualizados. |
| División | Hecho | `App/DTOs/{Auth,Compras,Inventario,Ventas,Contabilidad}`, `App/Interfaces/I*.cs`, `Dom/Enums/*Enums.cs`, `Dom/Entities/Maestros/{CatalogoEntities,ClienteEntity,CuentaContable}.cs`, controladores por dueño | `ErpDtos.cs`, `IErpServices.cs`, `ErpEnums.cs`, `MaestrosEntities.cs`, `MaestrosController.cs` y `ReportesController.cs` se dividieron por responsable (mismos namespaces y rutas). Cabecera `Módulo — Responsable` en cada archivo de código. |

### 1. Adán – Compras e Inventario

| ID | Estado | Archivos | Notas |
| :-- | :-- | :-- | :-- |
| A1 | Hecho | `App/DTOs/Compras/ComprasDtos.cs`, `Api/Controllers/ComprasController.cs` | `[Required]`, `MinLength(1)` en ítems, cantidades > 0; errores de negocio → 400 vía T5. |
| A2 | Hecho | `Api/Controllers/ProveedoresController.cs`, `App/DTOs/Compras/ProveedorDtos.cs` | CRUD con DTOs y baja lógica (bloqueada si hay OC abiertas). Rutas `/api/proveedores` y `/api/maestros/proveedores`. |
| A3 | Hecho | `Infra/Services/ComprasService.cs` | Regla documentada: el stock entra con la factura. Valida proveedor activo, factura duplicada, OC del mismo proveedor y no anulada/liquidada, recepción de la misma OC y **facturado ≤ recibido**. La OC queda `Liquidada` solo si todo se facturó. |
| A4 | Hecho | `Infra/Services/InventarioService.cs` (`CalcularCpp`) | Método estático testeable; stock ≤ 0 → CPP = costo de entrada. |
| A5 | Hecho | `Infra/Services/ComprasService.cs` | Saldos vía `ContabilidadService.AplicarMovimiento` (K3). |
| A6 | Hecho | `Infra/Services/InventarioService.cs`, `App/DTOs/Inventario/InventarioDtos.cs` | Asiento Merma/Pérdida (6.1.01.01) vs Inventario; `AsientoContableId` se asigna; `UsuarioEmail` eliminado del DTO. |
| A7 | Hecho | `InventarioService`, `InventarioController` | Filtro `desde/hasta` y paginación (`PaginaDto`, máx. 500). **Cambia la forma de la respuesta** (`items`, `totalRegistros`...). |
| A8 | Hecho (ronda 2) | `www/pantalla_gestion_proveedores.html` | Proveedores conectado a `/api/proveedores` con `apiFetch`. Pendiente: órdenes de compra, recepción/facturas, catálogo, kardex y ajustes (siguen con localStorage + `erp-sync`). |
| Extra | Hecho | `Api/Controllers/CatalogoController.cs` | Alta de productos/categorías con DTO (sin overposting de stock/CPP). |
| Visual | Hecho (ronda 2) | `www/pantalla_ordenes_compra.html`, `pantalla_recepcion_facturas_compra.html`, `pantalla_catalogo_productos.html`, `pantalla_kardex_inventario.html` | Badges de estado de OC, progreso de recepción por OC y por línea, resaltado de reorden. |
| Comentarios | Hecho | `IComprasService`, `ComprasService`, `InventarioService` | Regla del CPP y asiento de compra documentados en `/// <summary>`. |

### 2. María – Ventas

| ID | Estado | Archivos | Notas |
| :-- | :-- | :-- | :-- |
| M1 | Hecho | `Infra/Services/VentasService.cs` | El número se obtiene antes y se asigna directo a los movimientos creados; ya no se busca `"PENDIENTE"`. |
| M2 | Hecho | `VentasService`, `ContabilidadService.RevertirAsientoAsync` | Reversa de asientos de venta y costo. Se rechaza anular ventas con abonos. |
| M3 | Hecho | `VentasService` | Solo `Credito`: exige `LimiteCredito > 0` y deuda + venta ≤ límite. |
| M4 | Hecho | `VentasService`, `Api/Controllers/ReportesVentasController.cs` | Crédito vence a 30 días; contado vence el mismo día. `DiasMora` calculado en SQL. |
| M5 | Hecho | `App/Common/MetodosPago.cs` | Constantes + `Normalizar()` (se conserva la columna string, sin migración). **Cambio:** Tarjeta/Transferencia ahora quedan pagadas y van a Banco; antes quedaban como deuda. |
| M6 | Hecho | `App/DTOs/Ventas/VentasDtos.cs`, `VentasController` | `AnularVentaDto { motivo }` (colección Postman actualizada). |
| M7 | Hecho | `VentasDtos.cs`, `VentasService` | Cantidad > 0, descuento ≤ cantidad × precio, cliente y producto activos. |
| M8 | Hecho | `Api/Controllers/ClientesController.cs`, `App/DTOs/Ventas/ClienteDtos.cs` | CRUD con DTOs, sin `SaldoPendiente` en la entrada; baja bloqueada con deuda. Rutas `/api/clientes` y `/api/maestros/clientes`. |
| M9 | Hecho (ronda 2) | `www/pantalla_emision_ventas.html` | POS conectado: catálogo desde `/api/inventario/stock`, clientes desde `/api/clientes`, emisión `POST /api/ventas`. Ahora **exige cliente registrado**. Pendiente: historial, cobranzas (plazo/ventas) y reportes de ventas. |
| Visual | Hecho (ronda 2) | `www/pantalla_emision_ventas.html`, `pantalla_cobranzas_*.html`, `pantalla_historial_directas.html` | Stock por producto y bloqueo en POS, semáforo de mora, modal de anulación con motivo. |
| Comentarios | Hecho | `VentasService` | Cuentas del asiento de venta corregidas y flujo de abono en cascada documentado. |

### 3. Cristóbal – Contabilidad, Seguridad e Integración

| ID | Estado | Archivos | Notas |
| :-- | :-- | :-- | :-- |
| K1 | Hecho | `Infra/Services/ContabilidadService.cs`, `ContabilidadDtos.cs` | ≥ 2 líneas, cada línea solo Debe o Haber, cuentas existentes, activas y con `PermiteMovimiento`. |
| K2 | Hecho | `ContabilidadService` | Balance y estado de resultados agrupados en SQL. |
| K3 | Hecho | `ContabilidadService.AplicarMovimiento`, `RegistrarAsientoAutomaticoAsync` | Único punto que crea asientos y modifica saldos; Compras, Ventas e Inventario lo usan. Si falta una cuenta del plan, ahora se lanza error (antes se omitía el asiento en silencio). |
| K4 | Hecho | `Infra/Data/AuditoriaInterceptor.cs`, `Api/Infraestructura/UsuarioActualHttp.cs` | Interceptor de `SaveChanges` sobre documentos y maestros (usuario e IP). |
| K5 | Hecho | `App/Common/PermisosConst.cs`, `Api/Program.cs`, controladores, `DbInitializer` | Una política por permiso; Administrador cumple todas. El seed agrega permisos faltantes y asigna permisos por rol si `RolPermisos` está vacío. |
| K6 | Hecho (ronda 2) | `Api/Controllers/StoreController.cs`, `www/erp-sync.js` | Protegido con JWT y `DELETE` valida prefijo. Pendiente: retirar el puente cuando todas las pantallas usen la API. |
| K7 | Hecho | `Infra/Services/Seguridad/HashContrasenas.cs`, `AuthService`, `DbInitializer` | PBKDF2-SHA256 con sal; hashes SHA-256 antiguos se re-hashean en el primer login. |
| K8 | Hecho | `AuthService`, T5 | Correo duplicado / rol inexistente → 400. |
| K9 | Parcial | `ProveedoresController`, `ClientesController`, `AuthController` | Devuelven DTOs. Pendiente: órdenes, facturas, ventas, asientos y kardex siguen devolviendo entidades (no exponen `PasswordHash`; ciclos controlados con `IgnoreCycles`). |
| K10 | Hecho (ronda 2) | `www/pantalla_libro_diario.html` | Libro diario conectado (lista y alta de asientos; plan de cuentas desde la API). Anular asiento queda pendiente (no hay endpoint). Pendiente: libro mayor, plan de cuentas, estados financieros, reportes contables y administración de usuarios. |
| Visual | Hecho (ronda 2) | `www/pantalla_libro_diario.html`, `pantalla_plan_cuentas.html`, `pantalla_estados_financieros.html` | Cuadrado/descuadrado por asiento y totales, árbol colapsable, estados imprimibles. |
| Comentarios | Hecho | controladores contables, `ContabilidadEnums.cs` | `<summary>`/`<response>` y convención deudora/acreedora documentada. |

### Ronda 2 (todas las pantallas conectadas, perfil Cajero, login por rol y rediseño)

Respaldo previo: `ERP_backup_ronda2/` (src sin bin/obj). Build final `dotnet build ERPIntegrado.slnx` → **0 advertencias, 0 errores**.
Sin cambios de modelo (`has-pending-model-changes`: sin cambios) → **no se creó migración**. Las 22 piezas JSX (21 pantallas + `shared/erp-ui.jsx`) compilan con esbuild.
No se ejecutó la API contra la base de datos: el seed nuevo (rol Cajero, permisos, CAJA1 → Cajero) se aplicará en el próximo arranque.

**Transversal (Cristóbal)**

| Ítem | Estado | Archivos | Notas |
| :-- | :-- | :-- | :-- |
| Sistema de diseño | Hecho | `www/shared/erp-ui.css`, `www/shared/erp-ui.jsx` | Paleta por módulo (`data-modulo`), encabezado con ícono y migas, KPI, tablas, badges, buscador, estados vacíos/carga, modales y `confirmar()` (reemplaza `confirm()`), pestañas, progreso, CSV, impresión. |
| Registro de pantallas y permisos | Hecho | `www/shared/erp-common.js` | `ErpAuth.PAGINAS` define módulo, ícono y permiso de cada pantalla; guard de acceso → `acceso_denegado.html`; cierre automático al expirar el token. |
| Navbar por rol | Hecho | `www/shared/navbar.js` | Menú armado desde el registro y filtrado por permisos; usuario, rol y "Cerrar sesión". |
| Login por rol | Hecho | `www/login.html`, `Api/Controllers/AuthController.cs` | Redirección por rol; recuadro de credenciales demo desde `GET /api/auth/demo-credenciales` (solo Development). |
| Retiro de erp-sync | Hecho | `tools/obsoleto/erp-sync.js`, `Api/Controllers/StoreController.cs` | Ninguna pantalla lo carga; `StoreController` marcado `[Obsolete]` y restringido a administración. La tabla `ErpKeyValue` no se borró. |
| Enums como texto | Hecho | `Api/Program.cs` | `JsonStringEnumConverter` (acepta también números al recibir). **Cambio visible en Postman.** |

**Perfil Cajero (María / Cristóbal)**

| Ítem | Estado | Archivos | Notas |
| :-- | :-- | :-- | :-- |
| Rol y permisos | Hecho | `App/Common/PermisosConst.cs`, `Infra/Data/DbInitializer.cs` | Permisos exactos: `ventas.documentos.crear`, `ventas.productos.consultar`, `ventas.clientes.leer`, `ventas.clientes.crear`, `ventas.clientes.escribir`, `ventas.pagos.crear`, `ventas.caja.propias`. El seed los restablece en cada arranque (quita sobrantes). |
| Usuario cajero | Hecho | `DbInitializer.cs`, `appsettings.Development.json` | Reutiliza `CAJA1` (pasa a rol Cajero); si no existe crea `cajero@erp.com` con `Seed:CajeroPassword`. |
| Backend | Hecho | `VentasController` (`catalogo`, `pendientes`, `pagos/cascada`, `caja/hoy`), `ClientesController` (POST con `ventas.clientes.crear`) | El cajero no necesita permisos de Inventario para vender. |
| Frontend | Hecho | `www/pantalla_caja_inicio.html` (nueva) | Inicio de caja con ventas y abonos del día e impresión de cierre. |

**Adán**

| Pantalla | Endpoints | Notas |
| :-- | :-- | :-- |
| Proveedores | `GET/POST/PUT/DELETE /api/proveedores` | Rediseñada con KPI, filtros y modal. |
| Órdenes de compra | `GET/POST /api/compras/ordenes`, `POST /api/compras/ordenes/{id}/anular` (nuevo), `GET /api/proveedores`, `GET /api/maestros/productos` | Badges de estado, progreso de recepción, anulación con motivo. |
| Recepción y facturas | `GET/POST /api/compras/recepciones` (GET nuevo), `GET/POST /api/compras/facturas`, `GET /api/compras/ordenes` | Propone lo recibido sin facturar; confirmación antes de aprobar. |
| Catálogo de productos | `GET /api/inventario/stock`, `GET /api/inventario/valorizacion`, `GET/POST /api/maestros/categorias`, `POST/PUT/DELETE /api/maestros/productos` (PUT/DELETE nuevos) | Resaltado bajo reorden, margen sobre CPP. |
| Kardex | `GET /api/inventario/kardex/{id}` (paginado), `GET /api/inventario/stock` | Alerta de reorden, badges por tipo de movimiento. |
| Ajustes y alertas | `GET/POST /api/inventario/ajustes` (GET nuevo), `GET /api/inventario/alertas-reorden`, `GET /api/inventario/stock` | Vista previa del stock y del valor del ajuste; confirmación modal. |

**María**

| Pantalla | Endpoints | Notas |
| :-- | :-- | :-- |
| Mi caja (nueva) | `GET /api/ventas/caja/hoy` | Inicio del Cajero. |
| POS | `GET /api/ventas/catalogo`, `GET/POST /api/clientes`, `POST /api/ventas` | Stock por producto y bloqueo, cliente rápido, control de crédito, comprobante imprimible. |
| Clientes (verificada y rediseñada) | `GET/POST/PUT/DELETE /api/clientes` | Uso de la línea de crédito; edición solo con `ventas.clientes.escribir`. |
| Cobranzas y abonos | `GET /api/ventas/pendientes`, `POST /api/ventas/pagos`, `POST /api/ventas/pagos/cascada` (nuevos salvo pagos) | Semáforo de mora y abono en cascada. |
| Historial de ventas | `GET /api/ventas`, `POST /api/ventas/{id}/anular` | Filtros, detalle, comprobante imprimible, anulación con motivo, CSV. |
| Cuentas por cobrar | `GET /api/reportes/cuentas-por-cobrar` | Antigüedad por tramos y semáforo. |
| Reportes de ventas | `GET /api/reportes/resumen-ventas`, `ventas-por-dia`, `ventas-por-producto` (nuevos), `ventas-por-cliente` | Gráfico diario, rankings, CSV. |

**Cristóbal**

| Pantalla | Endpoints | Notas |
| :-- | :-- | :-- |
| Portal (index_erp) | `GET /api/dashboard/resumen` (nuevo) | Bloques según permisos del rol. |
| Plan de cuentas | `GET/POST /api/maestros/cuentas-contables`, `PUT /api/maestros/cuentas-contables/{id}` (nuevo) | Árbol colapsable por nivel con saldos acumulados. |
| Libro diario | `GET /api/contabilidad/libro-diario`, `POST /api/contabilidad/asientos` | Cuadrado/descuadrado por asiento, totales al pie, partida doble en vivo. |
| Libro mayor | `GET /api/contabilidad/libro-mayor/{id}` | Saldo acumulado según naturaleza. |
| Estados financieros | `GET /api/contabilidad/estado-resultados`, `GET /api/contabilidad/balance-general` (nuevo) | Imprimibles (`@media print`). |
| Reportes contables | `GET /api/contabilidad/balance-comprobacion`, `GET /api/integracion/estado` (nuevo) | Sumas y saldos con verificación. |
| Usuarios y roles | `GET/PUT /api/usuarios`, `POST /api/auth/register`, `GET /api/roles`, `PUT /api/roles/{id}/permisos`, `GET /api/permisos`, `GET /api/integracion/auditoria` (nuevos salvo register) | Roles Administrador y Cajero no editables. |
| Integración contable | `GET /api/integracion/configuracion`, `GET /api/integracion/estado` (nuevos) | Matriz de centralización en solo lectura. |
| Login / Acceso denegado | `POST /api/auth/login`, `GET /api/auth/demo-credenciales` | `acceso_denegado.html` es nueva. |

**Pendientes y riesgos de la ronda 2**
- Sin prueba en ejecución contra la BD; validar con los dos perfiles de demo.
- Anular asientos manuales y facturas de compra sigue sin endpoint.
- La matriz de cuentas de integración no es editable (por diseño).
- JSX y Tailwind siguen compilándose en el navegador (V2 parcial).

---

## Ronda 3 (octubre 2026): 5 pantallas + 5 reportes + 10 requerimientos funcionales por integrante

División vigente: **María** = Ventas · **Adán** = Compras · **Cristóbal** = Inventario, Contabilidad, Seguridad e Integración
(la de `arquitectura_erp_3_personas_dotnet.md` y del documento del Trabajo Final; los textos anteriores que asignan Inventario a Adán quedan
como historial de las rondas 1 y 2). Hoy hay **28 pantallas HTML** (3 auxiliares: login, acceso denegado y portal).
`dotnet build` → 0 errores. Se probó en ejecución contra SQL Server Express (flujo OC → aprobación → recepción → factura → reportes, venta y comisiones, auditoría).

| Integrante | Cambio | Archivos |
| :-- | :-- | :-- |
| María | Reporte de **comisiones sugeridas** por vendedor (RF-V10), % configurable | `Api/Controllers/ReportesVentasController.cs` (`GET /api/reportes/comisiones-ventas`), `www/pantalla_reportes_ventas.html` |
| Adán | **Aprobación de OC** (RF-C03): nuevo estado `Aprobada`; la recepción exige OC aprobada; la factura exige mercadería recibida | `Dom/Enums/ComprasEnums.cs`, `Infra/Services/ComprasService.cs`, `Api/Controllers/ComprasController.cs` (`POST /api/compras/ordenes/{id}/aprobar`), `www/pantalla_ordenes_compra.html` |
| Adán | `pantalla_recepcion_facturas_compra.html` se **separa** en `pantalla_recepcion_mercaderia.html` y `pantalla_facturas_compra.html` | `www/` |
| Adán | Pantalla nueva **Historial y detalle de compras** (RF-C08/C09) | `www/pantalla_historial_compras.html` |
| Adán | **5 reportes de compras** (RF-C10) con pantalla propia | `Api/Controllers/ReportesComprasController.cs` (`historial-precios-compra`, `cumplimiento-ordenes`, `consolidado-facturas-compra`, `cuentas-por-pagar`), `www/pantalla_reportes_compras.html` |
| Cristóbal | Pantalla de **valorización de inventario** con detalle por producto y categoría | `www/pantalla_valorizacion_inventario.html` |
| Cristóbal | Pantalla de **auditoría** con filtros (fecha, usuario, módulo, acción, entidad) | `Api/Controllers/IntegracionController.cs`, `www/pantalla_auditoria.html` |
| Transversal | Corrección: los `Range` decimales (`"0.0001"`, `"0.01"`) fallaban con la configuración regional es-CL y devolvían 400 al crear OC, recepciones, ventas y abonos; ahora usan cultura invariante | `App/DTOs/{Compras,Ventas,Inventario}/*.cs` |
| Transversal | Menú, tono `naranjo` de KPI y badge `Aprobada` | `www/shared/erp-common.js`, `erp-ui.jsx` |

**Limitaciones conocidas**
- Cuentas por pagar: el sistema no registra pagos a proveedores; el saldo de cada factura aprobada es su total.
- Las comisiones se calculan por el usuario que emitió la venta (`CreadoPor`); no existe un campo "vendedor" separado.
- Anular asientos manuales y facturas de compra sigue sin endpoint (RF-A09 cubre la reversa de asientos automáticos).
- Los datos de prueba de la verificación quedaron en la base local (proveedor, producto, cliente, una OC y una venta).
