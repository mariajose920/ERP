// Módulo: Transversal (Seguridad / Integración) — Responsable: Cristóbal
// Utilidades comunes para todas las pantallas del ERP (se carga PRIMERO, antes que React):
//   - Registro de pantallas (módulo, ícono, permisos requeridos) → menú y control de acceso por rol.
//   - Sesión: JWT de POST /api/auth/login, rol y permisos; expiración automática → login.
//   - apiFetch: fetch con "Authorization: Bearer <token>" y manejo de 401/403/ProblemDetails.
//   - Toast común (mostrarToast) y helpers de formato (formatCLP, formatNumero, formatFecha).
(function () {
    'use strict';

    // Claves de sesión (sin prefijo "erp_": nunca fueron datos de negocio).
    var CLAVE_TOKEN = 'auth_token';
    var CLAVE_USUARIO = 'auth_usuario';

    // ---------------------------------------------------------------------------
    // Registro de pantallas. "permisos": basta con tener UNO (el Administrador ve todo).
    // "grupo" ordena el menú. Una pantalla sin permisos solo exige sesión.
    // ---------------------------------------------------------------------------
    var VER_PORTAL = ['ventas.documentos.leer', 'compras.ordenes.leer', 'inventario.stock.leer',
        'contabilidad.asientos.leer', 'contabilidad.reportes.leer', 'usuarios.administrar'];

    var PAGINAS = {
        'index_erp.html': { titulo: 'Portal', icono: '🏠', modulo: 'inicio', grupo: 'Inicio', permisos: VER_PORTAL },
        'pantalla_caja_inicio.html': { titulo: 'Mi caja', icono: '🧾', modulo: 'ventas', grupo: 'Ventas', permisos: ['ventas.caja.propias'] },
        'pantalla_emision_ventas.html': { titulo: 'Punto de venta (POS)', icono: '🛒', modulo: 'ventas', grupo: 'Ventas', permisos: ['ventas.documentos.crear'] },
        'pantalla_gestion_clientes.html': { titulo: 'Clientes', icono: '👥', modulo: 'ventas', grupo: 'Ventas', permisos: ['ventas.clientes.leer'] },
        'pantalla_cobranzas_plazo.html': { titulo: 'Cobranzas y abonos', icono: '⏳', modulo: 'ventas', grupo: 'Ventas', permisos: ['ventas.pagos.crear'] },
        'pantalla_historial_directas.html': { titulo: 'Historial de ventas', icono: '📦', modulo: 'ventas', grupo: 'Ventas', permisos: ['ventas.documentos.leer'] },
        'pantalla_cobranzas_ventas.html': { titulo: 'Cuentas por cobrar', icono: '🚦', modulo: 'ventas', grupo: 'Ventas', permisos: ['ventas.documentos.leer'] },
        'pantalla_reportes_ventas.html': { titulo: 'Reportes de ventas', icono: '📊', modulo: 'ventas', grupo: 'Ventas', permisos: ['ventas.documentos.leer'] },
        'pantalla_gestion_proveedores.html': { titulo: 'Proveedores', icono: '🏢', modulo: 'compras', grupo: 'Compras', permisos: ['compras.proveedores.leer'] },
        'pantalla_ordenes_compra.html': { titulo: 'Órdenes de compra', icono: '📋', modulo: 'compras', grupo: 'Compras', permisos: ['compras.ordenes.leer'] },
        'pantalla_recepcion_mercaderia.html': { titulo: 'Recepción de mercadería', icono: '📥', modulo: 'compras', grupo: 'Compras', permisos: ['compras.recepciones.crear', 'compras.ordenes.leer'] },
        'pantalla_facturas_compra.html': { titulo: 'Facturas de compra', icono: '🧾', modulo: 'compras', grupo: 'Compras', permisos: ['compras.facturas.crear', 'compras.ordenes.leer'] },
        'pantalla_historial_compras.html': { titulo: 'Historial de compras', icono: '🗂️', modulo: 'compras', grupo: 'Compras', permisos: ['compras.ordenes.leer'] },
        'pantalla_reportes_compras.html': { titulo: 'Reportes de compras', icono: '📊', modulo: 'compras', grupo: 'Compras', permisos: ['compras.ordenes.leer'] },
        'pantalla_catalogo_productos.html': { titulo: 'Catálogo de productos', icono: '🏷️', modulo: 'inventario', grupo: 'Inventario', permisos: ['inventario.stock.leer'] },
        'pantalla_kardex_inventario.html': { titulo: 'Kardex', icono: '📈', modulo: 'inventario', grupo: 'Inventario', permisos: ['inventario.stock.leer'] },
        'pantalla_valorizacion_inventario.html': { titulo: 'Valorización', icono: '💰', modulo: 'inventario', grupo: 'Inventario', permisos: ['inventario.stock.leer'] },
        'pantalla_ajustes_alertas_inventario.html': { titulo: 'Ajustes y alertas', icono: '⚠️', modulo: 'inventario', grupo: 'Inventario', permisos: ['inventario.stock.leer'] },
        'pantalla_plan_cuentas.html': { titulo: 'Plan de cuentas', icono: '🌳', modulo: 'contabilidad', grupo: 'Contabilidad', permisos: ['contabilidad.asientos.leer'] },
        'pantalla_libro_diario.html': { titulo: 'Libro diario', icono: '📖', modulo: 'contabilidad', grupo: 'Contabilidad', permisos: ['contabilidad.asientos.leer'] },
        'pantalla_libro_mayor.html': { titulo: 'Libro mayor', icono: '📒', modulo: 'contabilidad', grupo: 'Contabilidad', permisos: ['contabilidad.asientos.leer'] },
        'pantalla_periodos_contables.html': { titulo: 'Períodos contables', icono: '🔒', modulo: 'contabilidad', grupo: 'Contabilidad', permisos: ['contabilidad.asientos.leer'] },
        'pantalla_estados_financieros.html': { titulo: 'Estados financieros', icono: '🧮', modulo: 'contabilidad', grupo: 'Contabilidad', permisos: ['contabilidad.reportes.leer'] },
        'pantalla_reportes_contabilidad.html': { titulo: 'Reportes contables', icono: '📑', modulo: 'contabilidad', grupo: 'Contabilidad', permisos: ['contabilidad.reportes.leer'] },
        'pantalla_admin_usuarios.html': { titulo: 'Usuarios y roles', icono: '🔐', modulo: 'admin', grupo: 'Administración', permisos: ['usuarios.administrar'] },
        'pantalla_auditoria.html': { titulo: 'Auditoría', icono: '📜', modulo: 'admin', grupo: 'Administración', permisos: ['usuarios.administrar'] },
        'pantalla_configuracion_integracion.html': { titulo: 'Integración contable', icono: '🔌', modulo: 'admin', grupo: 'Administración', permisos: ['contabilidad.asientos.leer'] }
    };
    var GRUPOS = ['Inicio', 'Ventas', 'Compras', 'Inventario', 'Contabilidad', 'Administración'];
    var PUBLICAS = ['login.html', 'acceso_denegado.html'];

    // ---------------------------------------------------------------------------
    // Sesión
    // ---------------------------------------------------------------------------

    /** Decodifica el payload de un JWT (la firma la valida la API). */
    function leerPayload(token) {
        try {
            var base64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
            var json = decodeURIComponent(atob(base64).split('').map(function (c) {
                return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
            }).join(''));
            return JSON.parse(json);
        } catch (e) {
            return null;
        }
    }

    /** Token vigente o null si no existe o expiró. */
    function obtenerToken() {
        var token = localStorage.getItem(CLAVE_TOKEN);
        if (!token) return null;
        var payload = leerPayload(token);
        if (!payload || (payload.exp && payload.exp * 1000 < Date.now())) {
            localStorage.removeItem(CLAVE_TOKEN);
            return null;
        }
        return token;
    }

    /** Guarda la respuesta de /api/auth/login: token, nombre, rol y permisos (para el menú). */
    function guardarSesion(respuestaLogin) {
        localStorage.setItem(CLAVE_TOKEN, respuestaLogin.token);
        localStorage.setItem(CLAVE_USUARIO, JSON.stringify({
            nombreCompleto: respuestaLogin.nombreCompleto,
            email: respuestaLogin.email,
            rol: respuestaLogin.rol,
            permisos: respuestaLogin.permisos || []
        }));
        localStorage.removeItem('isAuthenticated'); // marca del login falso antiguo
    }

    /** Usuario autenticado { nombreCompleto, email, rol, permisos } o null. */
    function usuarioActual() {
        try { return JSON.parse(localStorage.getItem(CLAVE_USUARIO)); } catch (e) { return null; }
    }

    function esAdministrador() {
        var u = usuarioActual();
        return !!u && u.rol === 'Administrador';
    }

    /** True si el usuario tiene el permiso (el Administrador los tiene todos). */
    function tienePermiso(codigo) {
        var u = usuarioActual();
        if (!u) return false;
        return u.rol === 'Administrador' || (u.permisos || []).indexOf(codigo) !== -1;
    }

    /** True si el usuario puede abrir la pantalla indicada. */
    function puedeVer(pagina) {
        if (PUBLICAS.indexOf(pagina) !== -1) return true;
        var info = PAGINAS[pagina];
        if (!info) return true;
        if (!info.permisos || info.permisos.length === 0) return !!obtenerToken();
        return info.permisos.some(tienePermiso);
    }

    /** Pantalla de inicio según el rol: portal para perfiles de gestión, caja para el Cajero. */
    function paginaInicio() {
        if (puedeVer('index_erp.html')) return 'index_erp.html';
        if (puedeVer('pantalla_caja_inicio.html')) return 'pantalla_caja_inicio.html';
        if (puedeVer('pantalla_emision_ventas.html')) return 'pantalla_emision_ventas.html';
        return 'acceso_denegado.html';
    }

    /** Cierra la sesión y vuelve al login (opcionalmente con un motivo visible). */
    function cerrarSesion(motivo) {
        localStorage.removeItem(CLAVE_TOKEN);
        localStorage.removeItem(CLAVE_USUARIO);
        localStorage.removeItem('isAuthenticated');
        window.location.href = 'login.html' + (motivo ? '?motivo=' + encodeURIComponent(motivo) : '');
    }

    function paginaActual() {
        return window.location.pathname.split('/').pop() || 'index_erp.html';
    }

    /** Exige sesión vigente y permiso para la pantalla actual; programa el cierre al expirar el token. */
    function exigirSesion() {
        var pagina = paginaActual();
        if (PUBLICAS.indexOf(pagina) !== -1) return;
        var token = obtenerToken();
        if (!token) {
            window.location.replace('login.html?motivo=sesion');
            return;
        }
        if (!puedeVer(pagina)) {
            window.location.replace('acceso_denegado.html?pagina=' + encodeURIComponent(pagina));
            return;
        }
        var payload = leerPayload(token);
        if (payload && payload.exp) {
            var restante = payload.exp * 1000 - Date.now();
            // setTimeout admite hasta ~24,8 días; el token dura horas.
            setTimeout(function () { cerrarSesion('expirada'); }, Math.max(0, Math.min(restante, 2147483000)));
        }
    }

    // ---------------------------------------------------------------------------
    // apiFetch
    // ---------------------------------------------------------------------------

    /**
     * Llama a la API con el JWT y devuelve el JSON (o null). Lanza Error con el mensaje de
     * ProblemDetails cuando la respuesta no es 2xx. 401 con token = sesión vencida → login.
     */
    async function apiFetch(url, opciones) {
        opciones = opciones || {};
        var headers = Object.assign({ 'Accept': 'application/json' }, opciones.headers || {});
        var body = opciones.body;
        if (body !== undefined && body !== null && typeof body !== 'string') {
            body = JSON.stringify(body);
            headers['Content-Type'] = 'application/json';
        }
        var token = obtenerToken();
        if (token) headers['Authorization'] = 'Bearer ' + token;

        var resp = await fetch(url, Object.assign({}, opciones, { headers: headers, body: body }));

        if (resp.status === 401 && token) {
            cerrarSesion('expirada');
            var expirada = new Error('Sesión expirada. Inicie sesión nuevamente.');
            expirada.status = 401;
            throw expirada;
        }

        var texto = await resp.text();
        var datos = null;
        if (texto) {
            try { datos = JSON.parse(texto); } catch (e) { datos = texto; }
        }

        if (!resp.ok) {
            var mensaje = (datos && (datos.detail || datos.error || datos.mensaje || datos.title)) || ('Error HTTP ' + resp.status);
            if (datos && datos.errors) {
                // Errores de validación de DataAnnotations: se muestran todos.
                mensaje = Object.keys(datos.errors).map(function (k) { return datos.errors[k].join(' '); }).join('\n');
            }
            if (resp.status === 403) mensaje = 'Su perfil no tiene permiso para esta acción.';
            var err = new Error(mensaje);
            err.status = resp.status;
            err.datos = datos;
            throw err;
        }
        return datos;
    }

    // ---------------------------------------------------------------------------
    // Toast
    // ---------------------------------------------------------------------------

    var COLORES_TOAST = { exito: '#065f46', error: '#9f1239', aviso: '#92400e', info: '#312e81' };

    function contenedorToasts() {
        var c = document.getElementById('erp-toasts');
        if (!c) {
            c = document.createElement('div');
            c.id = 'erp-toasts';
            c.setAttribute('role', 'status');
            c.setAttribute('aria-live', 'polite');
            c.className = 'no-print';
            c.style.cssText = 'position:fixed;top:1rem;right:1rem;z-index:9999;display:flex;flex-direction:column;gap:.5rem;max-width:min(24rem,calc(100vw - 2rem));';
            document.body.appendChild(c);
        }
        return c;
    }

    function tipoPorTexto(texto) {
        var t = String(texto).toLowerCase();
        if (/error|no se pud|inválid|insuficiente|no puede|❌/.test(t)) return 'error';
        if (/éxito|correctamente|registrad|guardad|creado|emitid|aprobad|✅/.test(t)) return 'exito';
        return 'info';
    }

    /** Notificación no bloqueante. tipo: 'exito' | 'error' | 'aviso' | 'info'. */
    function mostrarToast(mensaje, tipo, duracionMs) {
        var mostrar = function () {
            tipo = tipo || tipoPorTexto(mensaje);
            var t = document.createElement('div');
            t.style.cssText = 'background:' + (COLORES_TOAST[tipo] || COLORES_TOAST.info) +
                ';color:#fff;padding:.75rem 1rem;border-radius:.6rem;box-shadow:0 10px 15px -3px rgba(0,0,0,.25);' +
                'font:500 .875rem/1.35 system-ui,sans-serif;white-space:pre-line;cursor:pointer;opacity:0;transition:opacity .2s;';
            t.textContent = String(mensaje);
            t.title = 'Clic para cerrar';
            t.onclick = function () { t.remove(); };
            contenedorToasts().appendChild(t);
            requestAnimationFrame(function () { t.style.opacity = '1'; });
            setTimeout(function () {
                t.style.opacity = '0';
                setTimeout(function () { t.remove(); }, 250);
            }, duracionMs || 4500);
        };
        if (document.body) mostrar(); else document.addEventListener('DOMContentLoaded', mostrar);
    }

    // ---------------------------------------------------------------------------
    // Formato
    // ---------------------------------------------------------------------------

    var fmtCLP = new Intl.NumberFormat('es-CL', { style: 'currency', currency: 'CLP', maximumFractionDigits: 0 });
    var fmtNumero = new Intl.NumberFormat('es-CL', { maximumFractionDigits: 2 });

    function formatCLP(valor) { var n = Number(valor); return fmtCLP.format(isFinite(n) ? n : 0); }
    function formatNumero(valor) { var n = Number(valor); return fmtNumero.format(isFinite(n) ? n : 0); }

    /** dd-mm-aaaa (con hora si conHora). Acepta Date, ISO o timestamp. */
    function formatFecha(valor, conHora) {
        if (!valor) return '';
        var d = valor instanceof Date ? valor : new Date(valor);
        if (isNaN(d.getTime())) return String(valor);
        var opciones = { day: '2-digit', month: '2-digit', year: 'numeric' };
        if (conHora) { opciones.hour = '2-digit'; opciones.minute = '2-digit'; }
        return d.toLocaleString('es-CL', opciones);
    }

    /** Fecha local en formato aaaa-mm-dd (para inputs type=date). */
    function fechaISO(d) {
        d = d || new Date();
        var m = String(d.getMonth() + 1).padStart(2, '0');
        var dia = String(d.getDate()).padStart(2, '0');
        return d.getFullYear() + '-' + m + '-' + dia;
    }

    // ---------------------------------------------------------------------------
    // Exposición global
    // ---------------------------------------------------------------------------
    window.ErpAuth = {
        obtenerToken: obtenerToken,
        guardarSesion: guardarSesion,
        usuarioActual: usuarioActual,
        esAdministrador: esAdministrador,
        tienePermiso: tienePermiso,
        puedeVer: puedeVer,
        paginaInicio: paginaInicio,
        cerrarSesion: cerrarSesion,
        exigirSesion: exigirSesion,
        paginaActual: paginaActual,
        PAGINAS: PAGINAS,
        GRUPOS: GRUPOS
    };
    window.apiFetch = apiFetch;
    window.mostrarToast = mostrarToast;
    window.formatCLP = formatCLP;
    window.formatNumero = formatNumero;
    window.formatFecha = formatFecha;
    window.fechaISO = fechaISO;
    window.logout = function () { cerrarSesion(); };
    // Cualquier alert() residual se muestra como toast no bloqueante.
    window.alert = function (mensaje) { mostrarToast(mensaje); };

    // Paleta del módulo de la pantalla actual (la usa shared/erp-ui.css vía data-modulo).
    var info = PAGINAS[paginaActual()];
    if (info) document.documentElement.setAttribute('data-modulo', info.modulo);

    exigirSesion();
})();
