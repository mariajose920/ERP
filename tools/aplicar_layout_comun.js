// Módulo: Transversal (V1, V2, T2) — Responsable: Cristóbal
// OBSOLETO (ronda 2): las pantallas se reescribieron con shared/erp-ui.jsx y la navbar se arma desde ErpAuth.PAGINAS. Se conserva como referencia.
// Script de mantenimiento (se ejecutó una vez; es idempotente):
//   1. Genera wwwroot/shared/navbar.js a partir de la navbar de index_erp.html (V1).
//   2. En cada pantalla reemplaza la navbar duplicada por <div id="erp-navbar"> + navbar.js.
//   3. Reemplaza el login falso (isAuthenticated) por shared/erp-common.js (T2).
//   4. Fija versiones de CDN y usa builds production de React (V2).
//   5. Agrega el comentario de cabecera con módulo y responsable.
// Uso (desde la carpeta ERP/): node tools/aplicar_layout_comun.js
const fs = require('fs');
const path = require('path');

const raiz = path.join(__dirname, '..', 'src', 'ERP.Api', 'wwwroot');

// Dueño de cada pantalla (división por estudiante).
const DUENOS = {
    'pantalla_gestion_proveedores.html': ['Compras', 'Adán'],
    'pantalla_ordenes_compra.html': ['Compras', 'Adán'],
    'pantalla_recepcion_facturas_compra.html': ['Compras', 'Adán'],
    'pantalla_catalogo_productos.html': ['Inventario', 'Adán'],
    'pantalla_kardex_inventario.html': ['Inventario', 'Adán'],
    'pantalla_ajustes_alertas_inventario.html': ['Inventario', 'Adán'],
    'pantalla_gestion_clientes.html': ['Ventas', 'María'],
    'pantalla_emision_ventas.html': ['Ventas', 'María'],
    'pantalla_historial_directas.html': ['Ventas', 'María'],
    'pantalla_cobranzas_plazo.html': ['Ventas', 'María'],
    'pantalla_cobranzas_ventas.html': ['Ventas', 'María'],
    'pantalla_reportes_ventas.html': ['Ventas', 'María'],
    'pantalla_plan_cuentas.html': ['Contabilidad', 'Cristóbal'],
    'pantalla_libro_diario.html': ['Contabilidad', 'Cristóbal'],
    'pantalla_libro_mayor.html': ['Contabilidad', 'Cristóbal'],
    'pantalla_estados_financieros.html': ['Contabilidad', 'Cristóbal'],
    'pantalla_reportes_contabilidad.html': ['Contabilidad', 'Cristóbal'],
    'pantalla_admin_usuarios.html': ['Seguridad', 'Cristóbal'],
    'pantalla_configuracion_integracion.html': ['Integración', 'Cristóbal'],
    'index_erp.html': ['Portal / Integración', 'Cristóbal'],
    'login.html': ['Seguridad', 'Cristóbal']
};

// Versiones fijas de CDN (V2).
const CDN = [
    [/<script src="https:\/\/cdn\.tailwindcss\.com[^"]*"><\/script>/g,
        '<script src="https://cdn.tailwindcss.com/3.4.16"></script>'],
    [/<script src="https:\/\/unpkg\.com\/react@[^"]*"[^>]*><\/script>/g,
        '<script src="https://unpkg.com/react@18.3.1/umd/react.production.min.js" crossorigin></script>'],
    [/<script src="https:\/\/unpkg\.com\/react-dom@[^"]*"[^>]*><\/script>/g,
        '<script src="https://unpkg.com/react-dom@18.3.1/umd/react-dom.production.min.js" crossorigin></script>'],
    [/<script src="https:\/\/unpkg\.com\/@babel\/standalone[^"]*"[^>]*><\/script>/g,
        '<script src="https://unpkg.com/@babel/standalone@7.26.4/babel.min.js"></script>']
];

const INICIO_NAV = '<!-- NAVBAR RESPONSIVA UNIFICADA -->';
const FIN_NAV = '<!-- FIN NAVBAR -->';
const MARCA_NAV = '<div id="erp-navbar"></div>';

// 1. Generar navbar.js desde index_erp.html (solo si index aún tiene la navbar en línea).
const indexRuta = path.join(raiz, 'index_erp.html');
const indexHtml = fs.readFileSync(indexRuta, 'utf8');
const navMatch = indexHtml.match(/<nav[\s\S]*?<\/nav>/);
if (indexHtml.includes(INICIO_NAV) && navMatch) {
    let nav = navMatch[0]
        .replace('id="mobile-menu-btn" class=', 'id="mobile-menu-btn" aria-label="Abrir menú" class=')
        .replace(/`/g, '\\`');
    const js = `// Módulo: Transversal (V1) — Responsable: Cristóbal
// Navbar compartida: se inyecta en <div id="erp-navbar"></div> de cada pantalla.
// Para agregar o mover un enlace, edite SOLO este archivo (antes estaba copiada en 21 HTML).
(function () {
    'use strict';
    var html = \`${nav}\`;

    var destino = document.getElementById('erp-navbar');
    if (!destino) return;
    destino.outerHTML = html;

    // Usuario autenticado en la barra (si hay sesión).
    var usuario = window.ErpAuth && window.ErpAuth.usuarioActual();
    var menu = document.getElementById('desktop-menu');
    if (usuario && menu) {
        var etiqueta = document.createElement('span');
        etiqueta.className = 'ml-2 text-xs text-indigo-100';
        etiqueta.textContent = usuario.nombreCompleto + ' (' + usuario.rol + ')';
        menu.appendChild(etiqueta);
    }

    // Menú móvil.
    var btn = document.getElementById('mobile-menu-btn');
    var menuMovil = document.getElementById('mobile-menu');
    if (btn && menuMovil) {
        btn.addEventListener('click', function () {
            menuMovil.classList.toggle('hidden');
            var abierto = !menuMovil.classList.contains('hidden');
            btn.setAttribute('aria-label', abierto ? 'Cerrar menú' : 'Abrir menú');
            var iconMenu = document.getElementById('icon-menu');
            var iconClose = document.getElementById('icon-close');
            if (iconMenu) iconMenu.classList.toggle('hidden');
            if (iconClose) iconClose.classList.toggle('hidden');
        });
    }

    // Resaltar la pantalla actual.
    var actual = window.location.pathname.split('/').pop() || 'index_erp.html';
    document.querySelectorAll('.nav-link').forEach(function (link) {
        if (link.getAttribute('data-path') === actual) {
            link.classList.remove('text-indigo-200', 'text-slate-700', 'text-indigo-100');
            link.classList.add('bg-indigo-700', 'text-white', 'font-bold');
            link.setAttribute('aria-current', 'page');
        }
    });
})();
`;
    fs.mkdirSync(path.join(raiz, 'shared'), { recursive: true });
    fs.writeFileSync(path.join(raiz, 'shared', 'navbar.js'), js, 'utf8');
    console.log('shared/navbar.js generado');
}

// 2..5. Aplicar a cada pantalla.
for (const nombre of fs.readdirSync(raiz).filter(f => f.endsWith('.html'))) {
    const ruta = path.join(raiz, nombre);
    let html = fs.readFileSync(ruta, 'utf8').replace(/^﻿/, '');
    const original = html;

    // Cabecera de módulo/responsable.
    const [modulo, resp] = DUENOS[nombre] || ['Transversal', 'Cristóbal'];
    if (!html.includes('<!-- Módulo:')) {
        html = html.replace(/<head>/i, `<head>\n    <!-- Módulo: ${modulo} — Responsable: ${resp} -->`);
    }

    // CDN fijos.
    for (const [re, rep] of CDN) html = html.replace(re, rep);

    // Login falso -> erp-common.js (antes de erp-sync.js).
    html = html.replace(/\s*<script>\s*if \(localStorage\.getItem\('isAuthenticated'\)[\s\S]*?<\/script>/, '');
    if (nombre !== 'login.html' && !html.includes('shared/erp-common.js')) {
        if (html.includes('<script src="erp-sync.js"></script>')) {
            html = html.replace('<script src="erp-sync.js"></script>',
                '<script src="shared/erp-common.js"></script>\n    <script src="erp-sync.js"></script>');
        } else {
            html = html.replace(/<\/title>/, '</title>\n    <script src="shared/erp-common.js"></script>');
        }
    }

    // Navbar duplicada -> navbar compartida.
    const ini = html.indexOf(INICIO_NAV);
    const fin = html.lastIndexOf(FIN_NAV);
    if (ini !== -1 && fin > ini) {
        const inicioLinea = html.lastIndexOf('\n', ini) + 1;
        html = html.slice(0, inicioLinea) +
            `    <!-- Navbar compartida (V1): ver shared/navbar.js -->\n    ${MARCA_NAV}\n    <script src="shared/navbar.js"></script>` +
            html.slice(fin + FIN_NAV.length);
    }

    if (html !== original) {
        fs.writeFileSync(ruta, html, 'utf8');
        console.log('actualizado', nombre);
    }
}
