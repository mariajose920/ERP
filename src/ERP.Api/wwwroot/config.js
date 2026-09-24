/* ERP - Configuracion Local
   Este archivo configura la conexion del frontend con el backend local (ASP.NET Core).
   Al estar alojados en el mismo IIS/Servidor, se usan rutas relativas. */
window.ERP_CONFIG = {
  API_BASE_URL: '/api'
};


// Parse JWT and Protect Routes
(function protectRoutes() {
    if (window.location.pathname.endsWith('login.html')) return;
    
    const token = localStorage.getItem('token');
    if (!token) {
        window.location.href = 'login.html';
        return;
    }
    
    // Parse JWT Payload to get roles globally if needed
    try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        window.ERP_USER = payload;
    } catch (e) {
        localStorage.removeItem('token');
        window.location.href = 'login.html';
    }
})();
