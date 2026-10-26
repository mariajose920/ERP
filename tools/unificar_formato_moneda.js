// Módulo: Transversal (V5) — Responsable: Cristóbal
// Reemplaza las copias locales de "formatearMoneda" (Intl.NumberFormat 'es-CL' CLP) de cada pantalla
// por el helper común formatCLP() definido en wwwroot/shared/erp-common.js. Es idempotente.
// Uso (desde la carpeta ERP/): node tools/unificar_formato_moneda.js
const fs = require('fs');
const path = require('path');

const raiz = path.join(__dirname, '..', 'src', 'ERP.Api', 'wwwroot');
const patrones = [
    // Una línea: const formatearMoneda = (v) => new Intl.NumberFormat('es-CL', {...CLP...}).format(v);
    /const formatearMoneda = \((\w+)\) => new Intl\.NumberFormat\('es-CL', \{ style: 'currency', currency: 'CLP', maximumFractionDigits: 0 \}\)\.format\(\1\);/,
    // Bloque: const formatearMoneda = (v) => { [if (...) return "$0";] return new Intl...format(v [|| 0]); };
    /const formatearMoneda = \((\w+)\) => \{\s*(?:if \(![^\n]*\n\s*)?return new Intl\.NumberFormat\('es-CL', \{ style: 'currency', currency: 'CLP', maximumFractionDigits: 0 \}\)\.format\(\1(?: \|\| 0)?\);\s*\};/
];
const reemplazo = 'const formatearMoneda = formatCLP; // V5: helper común de shared/erp-common.js';

for (const nombre of fs.readdirSync(raiz).filter(f => f.endsWith('.html'))) {
    const ruta = path.join(raiz, nombre);
    let html = fs.readFileSync(ruta, 'utf8');
    const original = html;
    for (const re of patrones) html = html.replace(re, reemplazo);
    if (html !== original) {
        fs.writeFileSync(ruta, html, 'utf8');
        console.log('unificado', nombre);
    }
}
