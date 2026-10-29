// Módulo: Transversal (sistema de diseño) — Responsable: Cristóbal
// Componentes React comunes. Se carga con <script type="text/babel" src="shared/erp-ui.jsx">
// ANTES del script de cada pantalla, y expone todo en window (Pagina, Kpi, Tabla, Modal, confirmar...).
// Estilos en shared/erp-ui.css; paleta del módulo vía data-modulo (erp-common.js).
(function () {
    const { useState, useEffect, useCallback, useRef } = React;

    /** Carga una URL de la API con apiFetch. Devuelve { datos, cargando, error, recargar, setDatos }. */
    function useApi(url, inicial) {
        const [datos, setDatos] = useState(inicial === undefined ? null : inicial);
        const [cargando, setCargando] = useState(!!url);
        const [error, setError] = useState(null);
        const recargar = useCallback(async () => {
            if (!url) return;
            setCargando(true);
            setError(null);
            try {
                setDatos(await apiFetch(url));
            } catch (e) {
                setError(e.message);
                mostrarToast(e.message, 'error');
            } finally {
                setCargando(false);
            }
        }, [url]);
        useEffect(() => { recargar(); }, [recargar]);
        return { datos, cargando, error, recargar, setDatos };
    }

    /** Texto en minúsculas y sin tildes, para buscadores. */
    function normalizar(t) {
        return String(t == null ? '' : t).toLowerCase().normalize('NFD').replace(/[̀-ͯ]/g, '');
    }

    /** Distancia de edición (con transposición de letras adyacentes): "mraia" ↔ "maria" = 1. */
    function distanciaEdicion(a, b) {
        const d = [];
        for (let i = 0; i <= a.length; i++) { d[i] = [i]; }
        for (let j = 1; j <= b.length; j++) d[0][j] = j;
        for (let i = 1; i <= a.length; i++) for (let j = 1; j <= b.length; j++) {
            const costo = a[i - 1] === b[j - 1] ? 0 : 1;
            d[i][j] = Math.min(d[i - 1][j] + 1, d[i][j - 1] + 1, d[i - 1][j - 1] + costo);
            if (i > 1 && j > 1 && a[i - 1] === b[j - 2] && a[i - 2] === b[j - 1]) d[i][j] = Math.min(d[i][j], d[i - 2][j - 2] + 1);
        }
        return d[a.length][b.length];
    }

    /**
     * Buscador por similitud. Devuelve los elementos de `lista` que coinciden con lo escrito, primero los más parecidos.
     *  - Varias palabras: todas deben coincidir, en cualquier orden ("perez juan" encuentra "Juan Pérez").
     *  - Ignora mayúsculas y tildes; el RUT se encuentra con o sin puntos y guion.
     *  - Prioridad por palabra: igual > empieza con > una palabra empieza con > contiene > parecido (errores de tipeo).
     *  - `campos(x)` devuelve los textos a comparar; el 1.º pesa más (nombre), el 2.º un poco menos (RUT/SKU), el resto bastante menos.
     */
    function buscarSimilares(lista, q, campos, limite) {
        const consulta = normalizar(q).trim();
        if (!consulta) return limite ? lista.slice(0, limite) : lista;
        const limpiar = (t) => t.replace(/[.\-\s]/g, '');
        const tokens = consulta.split(/\s+/);
        const peso = (i) => i === 0 ? 0 : i === 1 ? 0.5 : 4;

        // Puntaje de un token contra un texto (menor = mejor); null si no se parece.
        const puntajeToken = (texto, tk, difuso) => {
            const t = normalizar(texto);
            if (!t) return null;
            const tkl = limpiar(tk), tl = limpiar(t);
            if (t === tk || (tkl && tl === tkl)) return 0;
            const palabras = t.split(/[\s\-.,@]+/).filter(Boolean);
            if (t.startsWith(tk) || (tkl.length >= 2 && tl.startsWith(tkl))) return 1;
            if (palabras.some(w => w.startsWith(tk))) return 2;
            if (t.includes(tk) || (tkl.length >= 3 && tl.includes(tkl))) return 3;
            if (difuso && tk.length >= 4 && !/\d/.test(tk)) {
                const maxDist = tk.length <= 5 ? 1 : 2;
                let mejor = Infinity;
                for (const w of palabras) mejor = Math.min(mejor, distanciaEdicion(tk, w), distanciaEdicion(tk, w.slice(0, tk.length)));
                if (mejor <= maxDist) return 3 + mejor;
            }
            return null;
        };
        const puntajeItem = (x, difuso) => {
            const textos = campos(x);
            let total = 0;
            for (const tk of tokens) {
                let mejor = null;
                textos.forEach((tx, i) => { const s = puntajeToken(tx, tk, difuso); if (s !== null) mejor = Math.min(mejor === null ? Infinity : mejor, s + peso(i)); });
                if (mejor === null) return null;
                total += mejor;
            }
            // Bonus si el nombre contiene lo escrito tal cual (frase continua).
            if (tokens.length > 1 && normalizar(textos[0]).includes(consulta)) total -= 0.5;
            return total / tokens.length;
        };

        const ordenar = (difuso) => lista.map(x => ({ x, p: puntajeItem(x, difuso) })).filter(r => r.p !== null)
            .sort((a, b) => a.p - b.p || normalizar(campos(a.x)[0]).localeCompare(normalizar(campos(b.x)[0])));
        // Primero coincidencias exactas/parciales; solo si no hay ninguna se admiten parecidos por errores de tipeo.
        let r = ordenar(false);
        if (!r.length) r = ordenar(true);
        r = r.map(i => i.x);
        return limite ? r.slice(0, limite) : r;
    }

    /** Contenedor de página: migas de pan (desde el registro de pantallas), ícono, título y acciones. */
    function Pagina({ icono, titulo, subtitulo, acciones, children }) {
        const pagina = ErpAuth.paginaActual();
        const info = ErpAuth.PAGINAS[pagina] || {};
        const inicio = ErpAuth.paginaInicio();
        return (
            <main className="erp-pagina">
                <nav className="erp-migas no-print" aria-label="Ruta">
                    <a href={inicio}>Inicio</a>
                    {info.grupo && info.grupo !== 'Inicio' && <><span aria-hidden="true">›</span><span>{info.grupo}</span></>}
                    <span aria-hidden="true">›</span><span className="mod-texto font-semibold">{titulo || info.titulo}</span>
                </nav>
                <header className="erp-encabezado">
                    <div className="flex items-center gap-3">
                        <div className="erp-encabezado-icono" aria-hidden="true">{icono || info.icono}</div>
                        <div>
                            <h1>{titulo || info.titulo}</h1>
                            {subtitulo && <p>{subtitulo}</p>}
                        </div>
                    </div>
                    {acciones && <div className="flex flex-wrap gap-2 no-print">{acciones}</div>}
                </header>
                {children}
            </main>
        );
    }

    const TONOS_KPI = {
        verde: ['#16a34a', '#dcfce7'], rojo: ['#e11d48', '#ffe4e6'], amarillo: ['#d97706', '#fef3c7'],
        azul: ['#2563eb', '#dbeafe'], violeta: ['#7c3aed', '#ede9fe'], naranjo: ['#ea580c', '#ffedd5'], gris: ['#64748b', '#f1f5f9']
    };

    /** Tarjeta KPI. tono: verde | rojo | amarillo | azul | violeta | gris (por defecto, color del módulo). */
    function Kpi({ icono, titulo, valor, detalle, tono, cargando }) {
        const t = TONOS_KPI[tono];
        const estilo = t ? { '--kpi-color': t[0], '--kpi-suave': t[1] } : undefined;
        return (
            <div className="erp-kpi" style={estilo}>
                <div className="erp-kpi-icono" aria-hidden="true">{icono}</div>
                <div className="min-w-0">
                    <div className="erp-kpi-titulo">{titulo}</div>
                    {cargando ? <div className="erp-esqueleto w-24 my-1.5"></div> : <div className="erp-kpi-valor">{valor}</div>}
                    {detalle && <div className="erp-kpi-detalle">{detalle}</div>}
                </div>
            </div>
        );
    }

    function Kpis({ children }) { return <section className="erp-kpis">{children}</section>; }

    /** Badge de estado. tono: verde | amarillo | rojo | azul | gris | violeta | naranjo | modulo. */
    function Badge({ tono, children, titulo }) {
        return <span className={`badge badge-${tono || 'gris'}`} title={titulo}>{children}</span>;
    }

    const ESTADOS_OC = {
        Pendiente: ['amarillo', 'Pendiente de aprobación'], Aprobada: ['naranjo', 'Aprobada'], RecibidaParcial: ['azul', 'Recepción parcial'],
        RecibidaTotal: ['violeta', 'Recibida total'], Liquidada: ['verde', 'Liquidada'], Anulada: ['rojo', 'Anulada']
    };
    /** Badge del estado de una orden de compra. */
    function BadgeEstadoOC({ estado }) {
        const [tono, texto] = ESTADOS_OC[estado] || ['gris', estado];
        return <Badge tono={tono}>{texto}</Badge>;
    }

    const ESTADOS_VENTA = {
        Emitida: ['amarillo', 'Pendiente de pago'], Pagada: ['verde', 'Pagada'],
        ParcialmentePagada: ['azul', 'Abono parcial'], Anulada: ['rojo', 'Anulada']
    };
    /** Badge del estado de una venta. */
    function BadgeEstadoVenta({ estado }) {
        const [tono, texto] = ESTADOS_VENTA[estado] || ['gris', estado];
        return <Badge tono={tono}>{texto}</Badge>;
    }

    /** Semáforo de mora: verde al día, amarillo 1–30 días, rojo más de 30. */
    function SemaforoMora({ dias }) {
        const d = Number(dias) || 0;
        if (d <= 0) return <Badge tono="verde">● Al día</Badge>;
        if (d <= 30) return <Badge tono="amarillo">● {d} días de mora</Badge>;
        return <Badge tono="rojo">● {d} días de mora</Badge>;
    }

    /** Estado del stock respecto del punto de reorden. */
    function BadgeStock({ stock, puntoReorden }) {
        const s = Number(stock) || 0;
        if (s <= 0) return <Badge tono="rojo">Sin stock</Badge>;
        if (s <= Number(puntoReorden || 0)) return <Badge tono="naranjo">⚠ Reponer</Badge>;
        return <Badge tono="verde">OK</Badge>;
    }

    /** Tarjeta con cabecera opcional. */
    function Card({ titulo, acciones, children, className, sinPadding }) {
        return (
            <section className={`erp-card ${className || ''}`}>
                {(titulo || acciones) && (
                    <div className="erp-card-cabecera">
                        {titulo && <h2>{titulo}</h2>}
                        {acciones && <div className="flex flex-wrap gap-2 items-center no-print">{acciones}</div>}
                    </div>
                )}
                <div className={sinPadding ? '' : 'erp-card-cuerpo'}>{children}</div>
            </section>
        );
    }

    /** Buscador con ícono. */
    function Buscador({ valor, onCambio, placeholder, etiqueta }) {
        return (
            <label className="relative block min-w-[14rem] flex-1">
                <span className="sr-only">{etiqueta || 'Buscar'}</span>
                <span className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" aria-hidden="true">🔍</span>
                <input type="search" className="erp-input" style={{ paddingLeft: '2.2rem' }} value={valor}
                    onChange={e => onCambio(e.target.value)} placeholder={placeholder || 'Buscar...'} />
            </label>
        );
    }

    /** Campo de formulario con label asociado (accesibilidad). */
    function Campo({ etiqueta, id, ayuda, error, children, className }) {
        return (
            <div className={`erp-campo ${className || ''}`}>
                <label htmlFor={id}>{etiqueta}</label>
                {children}
                {error ? <div className="erp-error">{error}</div> : ayuda && <div className="erp-ayuda">{ayuda}</div>}
            </div>
        );
    }

    /** Estado vacío. */
    function Vacio({ icono, titulo, texto, accion }) {
        return (
            <div className="erp-vacio">
                <div className="erp-vacio-icono" aria-hidden="true">{icono || '📭'}</div>
                <div className="font-semibold text-slate-700">{titulo || 'Sin datos'}</div>
                {texto && <div className="text-sm mt-1">{texto}</div>}
                {accion && <div className="mt-3">{accion}</div>}
            </div>
        );
    }

    /** Filas esqueleto mientras carga. */
    function Cargando({ filas }) {
        return (
            <div className="p-4 space-y-3" aria-busy="true" aria-label="Cargando">
                {Array.from({ length: filas || 4 }).map((_, i) => <div key={i} className="erp-esqueleto" style={{ width: `${90 - i * 12}%` }}></div>)}
            </div>
        );
    }

    /**
     * Tabla de datos. columnas: [{ titulo, campo?, render?(fila), num?, className? }].
     * Muestra esqueleto mientras carga y estado vacío si no hay filas.
     */
    function Tabla({ columnas, filas, cargando, vacio, pie, claveFila, onFila, filaClase }) {
        if (cargando) return <Cargando />;
        if (!filas || filas.length === 0) return vacio || <Vacio />;
        return (
            <div className="erp-tabla-contenedor">
                <table className="erp-tabla">
                    <thead><tr>{columnas.map((c, i) => <th key={i} className={c.num ? 'num' : ''}>{c.titulo}</th>)}</tr></thead>
                    <tbody>
                        {filas.map((f, idx) => (
                            <tr key={claveFila ? claveFila(f) : (f.id ?? idx)} onClick={onFila ? () => onFila(f) : undefined}
                                className={`${onFila ? 'cursor-pointer' : ''} ${filaClase ? filaClase(f) : ''}`}>
                                {columnas.map((c, i) => (
                                    <td key={i} className={`${c.num ? 'num' : ''} ${c.className || ''}`}>
                                        {c.render ? c.render(f) : f[c.campo]}
                                    </td>
                                ))}
                            </tr>
                        ))}
                    </tbody>
                    {pie && <tfoot>{pie}</tfoot>}
                </table>
            </div>
        );
    }

    /** Modal accesible (Escape cierra). imprimible: al imprimir se imprime solo el modal. */
    function Modal({ abierto, titulo, onCerrar, children, pie, ancho, imprimible }) {
        const ref = useRef(null);
        useEffect(() => {
            if (!abierto) return;
            const tecla = e => { if (e.key === 'Escape') onCerrar && onCerrar(); };
            document.addEventListener('keydown', tecla);
            setTimeout(() => {
                const primero = ref.current && ref.current.querySelector('input, select, textarea, button');
                if (primero) primero.focus();
            }, 30);
            return () => document.removeEventListener('keydown', tecla);
        }, [abierto]);
        if (!abierto) return null;
        return (
            <div className={`erp-modal-fondo ${imprimible ? "imprimible" : ""}`} onMouseDown={e => { if (e.target === e.currentTarget && onCerrar) onCerrar(); }}>
                <div ref={ref} className={`erp-modal ${ancho ? 'ancho' : ''}`} role="dialog" aria-modal="true" aria-label={titulo}>
                    <div className="erp-modal-cabecera">
                        <h3>{titulo}</h3>
                        {onCerrar && <button type="button" className="btn btn-secundario btn-icono" onClick={onCerrar} aria-label="Cerrar">✕</button>}
                    </div>
                    <div className="erp-modal-cuerpo">{children}</div>
                    {pie && <div className="erp-modal-pie">{pie}</div>}
                </div>
            </div>
        );
    }

    /**
     * Modal de confirmación (reemplaza confirm()). Uso: if (await confirmar({ titulo, mensaje })) {...}
     * Con pedirMotivo: true devuelve el texto ingresado (o null si se cancela).
     */
    function confirmar({ titulo, mensaje, textoConfirmar, peligro, pedirMotivo, etiquetaMotivo }) {
        return new Promise(resolve => {
            const host = document.createElement('div');
            document.body.appendChild(host);
            const raiz = ReactDOM.createRoot(host);
            const cerrar = valor => { raiz.unmount(); host.remove(); resolve(valor); };

            function Dialogo() {
                const [motivo, setMotivo] = useState('');
                const valido = !pedirMotivo || motivo.trim().length >= 3;
                return (
                    <Modal abierto titulo={titulo || 'Confirmar'} onCerrar={() => cerrar(pedirMotivo ? null : false)}
                        pie={<>
                            <button type="button" className="btn btn-secundario" onClick={() => cerrar(pedirMotivo ? null : false)}>Cancelar</button>
                            <button type="button" className={`btn ${peligro ? 'btn-peligro' : 'btn-primario'}`} disabled={!valido}
                                onClick={() => cerrar(pedirMotivo ? motivo.trim() : true)}>{textoConfirmar || 'Confirmar'}</button>
                        </>}>
                        <p className="text-sm text-slate-700 whitespace-pre-line">{mensaje}</p>
                        {pedirMotivo && (
                            <Campo etiqueta={etiquetaMotivo || 'Motivo'} id="confirmar-motivo" className="mt-4" ayuda="Mínimo 3 caracteres.">
                                <textarea id="confirmar-motivo" className="erp-input" rows="3" value={motivo} onChange={e => setMotivo(e.target.value)} />
                            </Campo>
                        )}
                    </Modal>
                );
            }
            raiz.render(<Dialogo />);
        });
    }

    /** Barra de progreso (p. ej. recibido / pedido). */
    function Progreso({ valor, maximo, etiqueta }) {
        const pct = maximo > 0 ? Math.min(100, Math.round((valor / maximo) * 100)) : 0;
        return (
            <div className="flex items-center gap-2" title={etiqueta}>
                <div className={`erp-progreso flex-1 ${pct >= 100 ? 'completo' : ''}`} role="progressbar" aria-valuenow={pct} aria-valuemin="0" aria-valuemax="100">
                    <span style={{ width: `${pct}%` }}></span>
                </div>
                <span className="text-xs font-semibold text-slate-600 w-10 text-right">{pct}%</span>
            </div>
        );
    }

    /** Pestañas. tabs: [{ id, texto }]. */
    function Tabs({ tabs, activa, onCambiar }) {
        return (
            <div className="erp-tabs no-print" role="tablist">
                {tabs.map(t => (
                    <button key={t.id} type="button" role="tab" aria-selected={activa === t.id}
                        className={`erp-tab ${activa === t.id ? 'activa' : ''}`} onClick={() => onCambiar(t.id)}>{t.texto}</button>
                ))}
            </div>
        );
    }

    /** Botón con estado de carga. */
    function Boton({ variante, cargando, children, className, ...props }) {
        return (
            <button type="button" className={`btn btn-${variante || 'primario'} ${className || ''}`} disabled={cargando || props.disabled} {...props}>
                {cargando && <span className="erp-spinner" aria-hidden="true"></span>}<span>{children}</span>
            </button>
        );
    }

    /** Descarga filas como CSV (separador ';' para Excel en español). columnas: [{ titulo, valor(fila) }]. */
    function descargarCSV(nombre, filas, columnas) {
        const esc = v => `"${String(v == null ? '' : v).replace(/"/g, '""')}"`;
        const lineas = [columnas.map(c => esc(c.titulo)).join(';')]
            .concat(filas.map(f => columnas.map(c => esc(c.valor(f))).join(';')));
        const blob = new Blob(['﻿' + lineas.join('\n')], { type: 'text/csv;charset=utf-8' });
        const a = document.createElement('a');
        a.href = URL.createObjectURL(blob);
        a.download = nombre;
        a.click();
        setTimeout(() => URL.revokeObjectURL(a.href), 1000);
    }

    /** Valida un RUT chileno (cuerpo + dígito verificador, con o sin puntos/guion). */
    function validarRut(rut) {
        const limpio = String(rut || '').replace(/[.\-\s]/g, '').toUpperCase();
        if (limpio.length < 8 || limpio.length > 9) return false;
        const cuerpo = limpio.slice(0, -1), dv = limpio.slice(-1);
        if (!/^\d+$/.test(cuerpo)) return false;
        let suma = 0, mul = 2;
        for (let i = cuerpo.length - 1; i >= 0; i--) { suma += Number(cuerpo[i]) * mul; mul = mul === 7 ? 2 : mul + 1; }
        const resto = 11 - (suma % 11);
        const esperado = resto === 11 ? '0' : resto === 10 ? 'K' : String(resto);
        return esperado === dv;
    }

    /** Normaliza un RUT al formato 12345678-9. */
    function formatearRut(rut) {
        const limpio = String(rut || '').replace(/[.\-\s]/g, '').toUpperCase();
        return limpio.length < 2 ? limpio : `${limpio.slice(0, -1)}-${limpio.slice(-1)}`;
    }

    /** Monta el componente raíz de la pantalla en #root. */
    // Evita la pantalla en blanco: si un componente falla al renderizar, se muestra el error y un botón para recargar.
    class LimiteError extends React.Component {
        constructor(props) { super(props); this.state = { error: null }; }
        static getDerivedStateFromError(error) { return { error }; }
        componentDidCatch(error, info) { console.error('Error de pantalla:', error, info && info.componentStack); }
        render() {
            if (!this.state.error) return this.props.children;
            return (
                <div className="p-6 max-w-2xl mx-auto">
                    <div className="erp-card p-6 border border-red-300 bg-red-50 text-red-800">
                        <h2 className="text-lg font-bold mb-2">⚠️ Ocurrió un error al mostrar esta pantalla</h2>
                        <p className="text-sm mb-3">Los datos guardados no se perdieron. Recargue la página; si se repite, informe este mensaje:</p>
                        <pre className="text-xs whitespace-pre-wrap bg-white p-3 rounded border border-red-200">{String(this.state.error && (this.state.error.stack || this.state.error.message || this.state.error))}</pre>
                        <button type="button" className="btn btn-primario mt-4" onClick={() => window.location.reload()}>Recargar</button>
                    </div>
                </div>
            );
        }
    }

    // Red de seguridad: si el DOM queda desincronizado de React (extensiones/traductores), recarga una vez en vez de dejar la pantalla en blanco.
    window.addEventListener('error', function (ev) {
        var msg = String(ev && ev.message || '');
        if (!/removeChild|insertBefore|not a child of this node/i.test(msg)) return;
        try {
            if (sessionStorage.getItem('erp_recarga_dom') === '1') { sessionStorage.removeItem('erp_recarga_dom'); return; }
            sessionStorage.setItem('erp_recarga_dom', '1');
        } catch (e) { return; }
        window.location.reload();
    });

    function montar(Componente) {
        ReactDOM.createRoot(document.getElementById('root')).render(<LimiteError><Componente /></LimiteError>);
    }

    Object.assign(window, {
        useApi, normalizar, buscarSimilares, Pagina, Kpi, Kpis, Badge, BadgeEstadoOC, BadgeEstadoVenta, SemaforoMora, BadgeStock,
        Card, Buscador, Campo, Vacio, Cargando, Tabla, Modal, confirmar, Progreso, Tabs, Boton, descargarCSV, montar,
        validarRut, formatearRut
    });
})();
