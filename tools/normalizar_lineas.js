// Módulo: Transversal (V4) — Responsable: Cristóbal
// Normaliza el artefacto de "línea en blanco entre cada línea" en los HTML de wwwroot.
// Regla: una línea en blanco aislada se elimina; una racha de 2+ líneas en blanco se reduce a 1.
// Uso (desde la carpeta ERP/): node tools/normalizar_lineas.js
const fs = require('fs');
const path = require('path');

const raiz = path.join(__dirname, '..', 'src', 'ERP.Api', 'wwwroot');
for (const nombre of fs.readdirSync(raiz).filter(f => f.endsWith('.html')).sort()) {
    const ruta = path.join(raiz, nombre);
    const lineas = fs.readFileSync(ruta, 'utf8').split(/\r?\n/);
    const vacias = lineas.filter(l => !l.trim()).length;
    if (!lineas.length || vacias / lineas.length < 0.3) continue; // sin artefacto
    const salida = [];
    let racha = 0;
    for (const l of lineas) {
        if (!l.trim()) { racha++; continue; }
        if (racha >= 2) salida.push('');
        racha = 0;
        salida.push(l.replace(/\s+$/, ''));
    }
    fs.writeFileSync(ruta, salida.join('\n') + '\n', 'utf8');
    console.log(`${nombre}: ${lineas.length} -> ${salida.length} líneas`);
}
