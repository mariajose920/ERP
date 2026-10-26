# Carpeta obsoleta

Esta carpeta es una copia antigua de las pantallas. **La fuente única del frontend es
`src/ERP.Api/wwwroot/`** (T8, ver `MEJORAS_PROPUESTAS.md`). No edite estos archivos: los cambios
aquí no se publican.

Al unificar (octubre 2026) solo dos archivos diferían de `wwwroot`; en ambos se conservó la versión
de esta carpeta por ser la más completa y se copió a `wwwroot`:

- `pantalla_libro_diario.html`: incluía el plan de cuentas de respaldo (`CUENTAS_INICIALES`) y los
  comentarios explicativos (centralización automática, partida doble, filtros, bloqueo de guardado).
- `pantalla_admin_usuarios.html`: incluía los permisos de Compras (`compras_proveedores`,
  `compras_ordenes`, `compras_stock`) en el rol Administrador y los comentarios explicativos; la copia
  de `wwwroot` solo tenía además líneas en blanco duplicadas.

Después de esa copia, `wwwroot` recibió nuevos cambios (navbar compartida, login con JWT, `apiFetch`,
toasts, CDN con versión fija, etc.) que **no** están aquí. La carpeta se mantiene solo como referencia
y puede eliminarse cuando el equipo lo decida.
