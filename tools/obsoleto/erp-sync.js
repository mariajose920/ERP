// Módulo: Transversal (puente localStorage → SQL) — Responsable: Cristóbal
// OBSOLETO (ronda 2): retirado de wwwroot; ninguna pantalla lo usa. Se conserva solo como referencia.
/* Sincroniza las claves erp_* de localStorage con SQL Server (api/store).
   Carga síncrona al inicio para que las pantallas lean los datos de la BD.
   Requiere shared/erp-common.js cargado antes (envía el JWT de la sesión).
   PENDIENTE (K6): retirar este puente cuando todas las pantallas usen la API real. */
(function () {
    var API = '/api/store';
    var nativeSet = Storage.prototype.setItem;
    var nativeRemove = Storage.prototype.removeItem;

    function token() {
        return window.ErpAuth ? window.ErpAuth.obtenerToken() : null;
    }

    function prepararPeticion(x) {
        var t = token();
        if (t) x.setRequestHeader('Authorization', 'Bearer ' + t);
    }

    function send(method, key, value) {
        if (!token()) return; // Sin sesión no se sincroniza.
        try {
            var x = new XMLHttpRequest();
            x.open(method, API + '/' + encodeURIComponent(key), true);
            x.setRequestHeader('Content-Type', 'application/json');
            prepararPeticion(x);
            x.send(method === 'PUT' ? JSON.stringify({ valor: value }) : null);
        } catch (e) { console.warn('erp-sync: no se pudo guardar', key, e); }
    }

    // 1. Cargar desde la BD (bloqueante)
    if (token()) {
        try {
            var x = new XMLHttpRequest();
            x.open('GET', API, false);
            prepararPeticion(x);
            x.send();
            if (x.status === 200) {
                var db = JSON.parse(x.responseText) || {};
                // Subir a la BD lo que solo existe en localStorage (migración inicial)
                for (var i = 0; i < localStorage.length; i++) {
                    var k = localStorage.key(i);
                    if (k.indexOf('erp_') === 0 && !(k in db)) send('PUT', k, localStorage.getItem(k));
                }
                // La BD es la fuente de verdad
                Object.keys(db).forEach(function (k) {
                    if (localStorage.getItem(k) !== db[k]) nativeSet.call(localStorage, k, db[k]);
                });
            } else if (x.status === 401 && window.ErpAuth) {
                window.ErpAuth.cerrarSesion();
            }
        } catch (e) { console.warn('erp-sync: API no disponible, se usa localStorage', e); }
    }

    // 2. Guardar en la BD cada escritura
    Storage.prototype.setItem = function (k, v) {
        nativeSet.call(this, k, v);
        if (this === localStorage && String(k).indexOf('erp_') === 0) send('PUT', k, String(v));
    };
    Storage.prototype.removeItem = function (k) {
        nativeRemove.call(this, k);
        if (this === localStorage && String(k).indexOf('erp_') === 0) send('DELETE', k);
    };
})();
