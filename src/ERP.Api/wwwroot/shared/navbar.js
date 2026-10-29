// Módulo: Transversal (V1) — Responsable: Cristóbal
// Navbar compartida: se inyecta en <div id="erp-navbar"></div>. El menú se arma desde el registro
// de pantallas de erp-common.js (ErpAuth.PAGINAS) y SOLO muestra lo que el rol del usuario puede ver
// (un Cajero ve únicamente Mi caja, POS, Clientes y Cobranzas).
(function () {
    'use strict';
    var destino = document.getElementById('erp-navbar');
    if (!destino || !window.ErpAuth) return;

    var auth = window.ErpAuth;
    var usuario = auth.usuarioActual() || { nombreCompleto: '', rol: '' };
    var actual = auth.paginaActual();
    var COLOR_GRUPO = { Inicio: '#a5b4fc', Ventas: '#6ee7b7', Compras: '#fcd34d', Inventario: '#7dd3fc', Contabilidad: '#c4b5fd', 'Administración': '#fda4af' };

    function escapar(t) {
        return String(t == null ? '' : t).replace(/[&<>"']/g, function (c) {
            return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c];
        });
    }

    // Pantallas visibles agrupadas.
    var grupos = auth.GRUPOS.map(function (g) {
        var items = Object.keys(auth.PAGINAS).filter(function (p) {
            return auth.PAGINAS[p].grupo === g && auth.puedeVer(p);
        });
        return { nombre: g, items: items };
    }).filter(function (g) { return g.items.length > 0; });

    function enlace(p, movil) {
        var info = auth.PAGINAS[p];
        var activo = p === actual;
        return '<a href="' + p + '" class="erp-nav-link' + (activo ? ' activo' : '') + (movil ? ' movil' : '') + '"' +
            (activo ? ' aria-current="page"' : '') + '><span aria-hidden="true">' + info.icono + '</span> ' + escapar(info.titulo) + '</a>';
    }

    var escritorio = grupos.map(function (g) {
        if (g.items.length === 1) return enlace(g.items[0], false);
        var activoGrupo = g.items.indexOf(actual) !== -1;
        return '<div class="erp-nav-grupo">' +
            '<button type="button" class="erp-nav-boton' + (activoGrupo ? ' activo' : '') + '" aria-haspopup="true">' +
            '<span class="erp-nav-punto" style="background:' + COLOR_GRUPO[g.nombre] + '"></span>' + escapar(g.nombre) + ' ▾</button>' +
            '<div class="erp-nav-menu" role="menu">' + g.items.map(function (p) { return enlace(p, false); }).join('') + '</div></div>';
    }).join('');

    // Menú móvil: grupos plegables (uno abierto a la vez; se abre el de la pantalla actual) para que el
    // panel sea corto y, si aun así no cabe, haga scroll por dentro.
    var movil = grupos.map(function (g) {
        if (g.items.length === 1) return enlace(g.items[0], true);
        var abierto = g.items.indexOf(actual) !== -1;
        return '<details class="erp-nav-acordeon" name="erp-nav-grupo-movil"' + (abierto ? ' open' : '') + '>' +
            '<summary class="erp-nav-titulo" style="color:' + COLOR_GRUPO[g.nombre] + '">' + escapar(g.nombre) +
            ' <span class="erp-nav-cantidad">' + g.items.length + '</span></summary>' +
            '<div class="erp-nav-lista">' + g.items.map(function (p) { return enlace(p, true); }).join('') + '</div></details>';
    }).join('');

    var inicio = auth.paginaInicio();
    destino.outerHTML =
        '<nav class="erp-nav no-print" aria-label="Menú principal">' +
        '  <div class="erp-nav-barra">' +
        '    <a href="' + inicio + '" class="erp-nav-marca"><span aria-hidden="true">🏢</span><span class="erp-nav-marca-texto">ERP CORPORATIVO</span></a>' +
        '    <div class="erp-nav-escritorio">' + escritorio + '</div>' +
        '    <div class="erp-nav-usuario">' +
        '      <span class="erp-nav-avatar" aria-hidden="true">' + escapar((usuario.nombreCompleto || '?').charAt(0)) + '</span>' +
        '      <span class="erp-nav-datos"><strong>' + escapar(usuario.nombreCompleto) + '</strong><small>' + escapar(usuario.rol) + '</small></span>' +
        '      <button type="button" id="erp-btn-salir" class="erp-nav-salir">Cerrar sesión</button>' +
        '      <button type="button" id="erp-btn-menu" class="erp-nav-hamburguesa" aria-label="Abrir menú" aria-expanded="false" aria-controls="erp-menu-movil">☰</button>' +
        '    </div>' +
        '  </div>' +
        '  <div id="erp-menu-fondo" class="erp-nav-fondo" hidden></div>' +
        '  <div id="erp-menu-movil" class="erp-nav-movil" hidden>' +
        '    <div class="erp-nav-movil-usuario"><span class="erp-nav-avatar" aria-hidden="true">' + escapar((usuario.nombreCompleto || '?').charAt(0)) + '</span>' +
        '      <span class="erp-nav-datos"><strong>' + escapar(usuario.nombreCompleto) + '</strong><small>' + escapar(usuario.rol) + '</small></span></div>' +
        movil +
        '    <button type="button" id="erp-btn-salir-movil" class="erp-nav-salir erp-nav-salir-movil">Cerrar sesión</button>' +
        '  </div>' +
        '</nav>';

    document.getElementById('erp-btn-salir').addEventListener('click', function () { auth.cerrarSesion(); });
    document.getElementById('erp-btn-salir-movil').addEventListener('click', function () { auth.cerrarSesion(); });
    var btnMenu = document.getElementById('erp-btn-menu');
    var menuMovil = document.getElementById('erp-menu-movil');
    var fondo = document.getElementById('erp-menu-fondo');

    function alternarMenu(abrir) {
        menuMovil.toggleAttribute('hidden', !abrir);
        fondo.toggleAttribute('hidden', !abrir);
        btnMenu.setAttribute('aria-expanded', String(abrir));
        btnMenu.setAttribute('aria-label', abrir ? 'Cerrar menú' : 'Abrir menú');
        btnMenu.textContent = abrir ? '✕' : '☰';
        document.documentElement.classList.toggle('erp-nav-abierto', abrir);
    }
    btnMenu.addEventListener('click', function () { alternarMenu(menuMovil.hasAttribute('hidden')); });
    fondo.addEventListener('click', function () { alternarMenu(false); });
    document.addEventListener('keydown', function (e) { if (e.key === 'Escape' && !menuMovil.hasAttribute('hidden')) { alternarMenu(false); btnMenu.focus(); } });
    // Barra compacta al bajar la página. Dos umbrales (histéresis) evitan el parpadeo: el cambio de alto
    // mueve el contenido unos píxeles y, con un solo umbral, podría reactivarse en bucle.
    var barra = document.querySelector('.erp-nav');
    function ajustarBarra() {
        var y = window.scrollY || document.documentElement.scrollTop;
        var compacto = barra.classList.contains('erp-nav-compacto');
        if (!compacto && y > 80) barra.classList.add('erp-nav-compacto');
        else if (compacto && y < 20) barra.classList.remove('erp-nav-compacto');
    }
    window.addEventListener('scroll', ajustarBarra, { passive: true });
    ajustarBarra();

    // Al ensanchar la ventana hasta el modo escritorio el panel móvil no debe quedar abierto.
    window.matchMedia('(min-width: 1200px)').addEventListener('change', function (e) { if (e.matches) alternarMenu(false); });
})();
