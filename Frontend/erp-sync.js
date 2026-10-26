/* Sincroniza las claves erp_* de localStorage con SQL Server (api/store).
   Carga sincrona al inicio para que las pantallas lean los datos de la BD. */
(function () {
    var API = '/api/store';
    var nativeSet = Storage.prototype.setItem;
    var nativeRemove = Storage.prototype.removeItem;
    var queue = {};

    function send(method, key, value) {
        try {
            var x = new XMLHttpRequest();
            x.open(method, API + '/' + encodeURIComponent(key), true);
            x.setRequestHeader('Content-Type', 'application/json');
            x.send(method === 'PUT' ? JSON.stringify({ valor: value }) : null);
        } catch (e) { console.warn('erp-sync: no se pudo guardar', key, e); }
    }

    // 1. Cargar desde la BD (bloqueante)
    try {
        var x = new XMLHttpRequest();
        x.open('GET', API, false);
        x.send();
        if (x.status === 200) {
            var db = JSON.parse(x.responseText) || {};
            // Subir a la BD lo que solo existe en localStorage (migracion inicial)
            for (var i = 0; i < localStorage.length; i++) {
                var k = localStorage.key(i);
                if (k.indexOf('erp_') === 0 && !(k in db)) send('PUT', k, localStorage.getItem(k));
            }
            // La BD es la fuente de verdad
            Object.keys(db).forEach(function (k) {
                if (localStorage.getItem(k) !== db[k]) nativeSet.call(localStorage, k, db[k]);
            });
        }
    } catch (e) { console.warn('erp-sync: API no disponible, se usa localStorage', e); }

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
