import os
import re

dir_path = r"C:\Users\mjvil\OneDrive\Escritorio\ERP_Osvaldo\ERP\src\ERP.Api\wwwroot"

new_nav = """    <!-- NAVBAR RESPONSIVA UNIFICADA -->
    <nav class="bg-indigo-900 text-white shadow-lg sticky top-0 z-50">
        <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div class="flex items-center justify-between h-16">
                <div class="flex items-center gap-3">
                    <a href="index_erp.html" class="flex items-center gap-2 hover:opacity-90 transition-opacity">
                        <span class="text-2xl">🏢</span>
                        <span class="font-bold text-xl tracking-wider text-white hidden sm:inline">ERP CORPORATIVO</span>
                    </a>
                </div>
                
                <div class="hidden xl:flex items-center space-x-1" id="desktop-menu">
                    <a href="index_erp.html" class="nav-link px-2.5 py-1.5 rounded-md text-xs font-semibold text-indigo-200 hover:bg-indigo-800 hover:text-white transition-colors" data-path="index_erp.html">🏠 Inicio</a>
                    
                    <!-- Dropdown Ventas -->
                    <div class="relative group">
                        <button class="px-2.5 py-1.5 rounded-md text-xs font-semibold text-indigo-200 hover:bg-indigo-800 hover:text-white transition-colors inline-flex items-center gap-1">
                            <span>🛒 Ventas (María)</span>
                            <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"></path></svg>
                        </button>
                        <div class="absolute left-0 w-52 rounded-lg shadow-xl bg-indigo-950 border border-indigo-800 hidden group-hover:block z-50 py-1.5 mt-0">
                            <a href="pantalla_gestion_clientes.html" class="nav-link block px-3.5 py-1.5 text-xs text-indigo-100 hover:bg-indigo-800 hover:text-white" data-path="pantalla_gestion_clientes.html">👥 Directorio Clientes</a>
                            <a href="pantalla_emision_ventas.html" class="nav-link block px-3.5 py-1.5 text-xs text-indigo-100 hover:bg-indigo-800 hover:text-white" data-path="pantalla_emision_ventas.html">🧾 Emisión POS Ventas</a>
                            <a href="pantalla_historial_directas.html" class="nav-link block px-3.5 py-1.5 text-xs text-indigo-100 hover:bg-indigo-800 hover:text-white" data-path="pantalla_historial_directas.html">📦 Ventas Directas</a>
                            <a href="pantalla_cobranzas_plazo.html" class="nav-link block px-3.5 py-1.5 text-xs text-indigo-100 hover:bg-indigo-800 hover:text-white" data-path="pantalla_cobranzas_plazo.html">⏳ Cobranzas Plazo</a>
                            <a href="pantalla_reportes_ventas.html" class="nav-link block px-3.5 py-1.5 text-xs text-indigo-100 hover:bg-indigo-800 hover:text-white" data-path="pantalla_reportes_ventas.html">📊 Reportes Ventas</a>
                        </div>
                    </div>

                    <!-- Dropdown Compras -->
                    <div class="relative group">
                        <button class="px-2.5 py-1.5 rounded-md text-xs font-semibold text-indigo-200 hover:bg-indigo-800 hover:text-white transition-colors inline-flex items-center gap-1">
                            <span>🛍️ Compras (Adan)</span>
                            <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"></path></svg>
                        </button>
                        <div class="absolute left-0 w-52 rounded-lg shadow-xl bg-indigo-950 border border-indigo-800 hidden group-hover:block z-50 py-1.5 mt-0">
                            <a href="pantalla_gestion_proveedores.html" class="nav-link block px-3.5 py-1.5 text-xs text-indigo-100 hover:bg-indigo-800 hover:text-white" data-path="pantalla_gestion_proveedores.html">🏢 Proveedores</a>
                            <a href="pantalla_ordenes_compra.html" class="nav-link block px-3.5 py-1.5 text-xs text-indigo-100 hover:bg-indigo-800 hover:text-white" data-path="pantalla_ordenes_compra.html">📋 Órdenes de Compra</a>
                            <a href="pantalla_recepcion_facturas_compra.html" class="nav-link block px-3.5 py-1.5 text-xs text-indigo-100 hover:bg-indigo-800 hover:text-white" data-path="pantalla_recepcion_facturas_compra.html">📄 Recepción Facturas</a>
                        </div>
                    </div>

                    <!-- Dropdown Inventario -->
                    <div class="relative group">
                        <button class="px-2.5 py-1.5 rounded-md text-xs font-semibold text-indigo-200 hover:bg-indigo-800 hover:text-white transition-colors inline-flex items-center gap-1">
                            <span>📦 Inventario (Adan)</span>
                            <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"></path></svg>
                        </button>
                        <div class="absolute left-0 w-56 rounded-lg shadow-xl bg-indigo-950 border border-indigo-800 hidden group-hover:block z-50 py-1.5 mt-0">
                            <a href="pantalla_catalogo_productos.html" class="nav-link block px-3.5 py-1.5 text-xs text-indigo-100 hover:bg-indigo-800 hover:text-white" data-path="pantalla_catalogo_productos.html">🏷️ Catálogo SKU</a>
                            <a href="pantalla_kardex_inventario.html" class="nav-link block px-3.5 py-1.5 text-xs text-indigo-100 hover:bg-indigo-800 hover:text-white" data-path="pantalla_kardex_inventario.html">📈 Kardex & Stock</a>
                            <a href="pantalla_ajustes_alertas_inventario.html" class="nav-link block px-3.5 py-1.5 text-xs text-indigo-100 hover:bg-indigo-800 hover:text-white" data-path="pantalla_ajustes_alertas_inventario.html">⚠️ Ajustes & Alertas</a>
                        </div>
                    </div>

                    <!-- Dropdown Contabilidad -->
                    <div class="relative group">
                        <button class="px-2.5 py-1.5 rounded-md text-xs font-semibold text-indigo-200 hover:bg-indigo-800 hover:text-white transition-colors inline-flex items-center gap-1">
                            <span>📊 Contabilidad</span>
                            <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"></path></svg>
                        </button>
                        <div class="absolute right-0 w-60 rounded-lg shadow-xl bg-indigo-950 border border-indigo-800 hidden group-hover:block z-50 py-1.5 mt-0">
                            <a href="pantalla_plan_cuentas.html" class="nav-link block px-3.5 py-1.5 text-xs text-indigo-100 hover:bg-indigo-800 hover:text-white" data-path="pantalla_plan_cuentas.html">📑 Plan de Cuentas</a>
                            <a href="pantalla_libro_diario.html" class="nav-link block px-3.5 py-1.5 text-xs text-indigo-100 hover:bg-indigo-800 hover:text-white" data-path="pantalla_libro_diario.html">📖 Libro Diario & Mayor</a>
                            <a href="pantalla_estados_financieros.html" class="nav-link block px-3.5 py-1.5 text-xs text-indigo-100 hover:bg-indigo-800 hover:text-white" data-path="pantalla_estados_financieros.html">📈 Estados Financieros</a>
                        </div>
                    </div>
                    
                    <!-- Dropdown Admin -->
                    <div class="relative group">
                        <button class="px-2.5 py-1.5 rounded-md text-xs font-semibold text-indigo-200 hover:bg-indigo-800 hover:text-white transition-colors inline-flex items-center gap-1">
                            <span>⚙️ Admin</span>
                            <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"></path></svg>
                        </button>
                        <div class="absolute right-0 w-60 rounded-lg shadow-xl bg-indigo-950 border border-indigo-800 hidden group-hover:block z-50 py-1.5 mt-0">
                            <a href="pantalla_admin_usuarios.html" class="nav-link block px-3.5 py-1.5 text-xs text-indigo-100 hover:bg-indigo-800 hover:text-white" data-path="pantalla_admin_usuarios.html">👥 Gestión Usuarios</a>
                            <a href="pantalla_configuracion_integracion.html" class="nav-link block px-3.5 py-1.5 text-xs text-indigo-100 hover:bg-indigo-800 hover:text-white" data-path="pantalla_configuracion_integracion.html">🔌 Configuración & Integración</a>
                        </div>
                    </div>
                    
                    <button onclick="logout()" class="px-2.5 py-1.5 rounded-md text-xs font-bold text-red-300 hover:bg-red-800 hover:text-white transition-colors ml-2">🚪 Salir</button>
                </div>

                <div class="xl:hidden">
                    <button id="mobile-menu-btn" class="inline-flex items-center justify-center p-2 rounded-md text-indigo-200 hover:text-white hover:bg-indigo-800 focus:outline-none">
                        <svg class="h-6 w-6" stroke="currentColor" fill="none" viewBox="0 0 24 24">
                            <path id="icon-menu" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
                            <path id="icon-close" class="hidden" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                        </svg>
                    </button>
                </div>
            </div>
        </div>
        
        <div id="mobile-menu" class="hidden xl:hidden bg-indigo-950 border-t border-indigo-800 px-4 pt-2 pb-4 space-y-2">
            <a href="index_erp.html" class="nav-link block px-3 py-1.5 rounded-md text-sm font-medium text-indigo-200 hover:bg-indigo-800 hover:text-white" data-path="index_erp.html">🏠 Inicio</a>
            
            <div class="pt-2 border-t border-indigo-800">
                <div class="text-xs font-semibold text-indigo-300 uppercase tracking-wider px-3 mb-1">🛒 Ventas (María)</div>
                <a href="pantalla_gestion_clientes.html" class="nav-link block px-3 py-1 rounded-md text-sm text-indigo-200 hover:bg-indigo-800 hover:text-white" data-path="pantalla_gestion_clientes.html">👥 Directorio Clientes</a>
                <a href="pantalla_emision_ventas.html" class="nav-link block px-3 py-1 rounded-md text-sm text-indigo-200 hover:bg-indigo-800 hover:text-white" data-path="pantalla_emision_ventas.html">🧾 Emisión POS Ventas</a>
                <a href="pantalla_historial_directas.html" class="nav-link block px-3 py-1 rounded-md text-sm text-indigo-200 hover:bg-indigo-800 hover:text-white" data-path="pantalla_historial_directas.html">📦 Ventas Directas</a>
                <a href="pantalla_cobranzas_plazo.html" class="nav-link block px-3 py-1 rounded-md text-sm text-indigo-200 hover:bg-indigo-800 hover:text-white" data-path="pantalla_cobranzas_plazo.html">⏳ Cobranzas Plazo</a>
                <a href="pantalla_reportes_ventas.html" class="nav-link block px-3 py-1 rounded-md text-sm text-indigo-200 hover:bg-indigo-800 hover:text-white" data-path="pantalla_reportes_ventas.html">📊 Reportes Ventas</a>
            </div>

            <div class="pt-2 border-t border-indigo-800">
                <div class="text-xs font-semibold text-emerald-300 uppercase tracking-wider px-3 mb-1">🛍️ Compras (Adan)</div>
                <a href="pantalla_gestion_proveedores.html" class="nav-link block px-3 py-1 rounded-md text-sm text-indigo-200 hover:bg-indigo-800 hover:text-white" data-path="pantalla_gestion_proveedores.html">🏢 Proveedores</a>
                <a href="pantalla_ordenes_compra.html" class="nav-link block px-3 py-1 rounded-md text-sm text-indigo-200 hover:bg-indigo-800 hover:text-white" data-path="pantalla_ordenes_compra.html">📋 Órdenes de Compra</a>
                <a href="pantalla_recepcion_facturas_compra.html" class="nav-link block px-3 py-1 rounded-md text-sm text-indigo-200 hover:bg-indigo-800 hover:text-white" data-path="pantalla_recepcion_facturas_compra.html">📄 Recepción Facturas</a>
            </div>

            <div class="pt-2 border-t border-indigo-800">
                <div class="text-xs font-semibold text-cyan-300 uppercase tracking-wider px-3 mb-1">📦 Inventario (Adan)</div>
                <a href="pantalla_catalogo_productos.html" class="nav-link block px-3 py-1 rounded-md text-sm text-indigo-200 hover:bg-indigo-800 hover:text-white" data-path="pantalla_catalogo_productos.html">🏷️ Catálogo SKU</a>
                <a href="pantalla_kardex_inventario.html" class="nav-link block px-3 py-1 rounded-md text-sm text-indigo-200 hover:bg-indigo-800 hover:text-white" data-path="pantalla_kardex_inventario.html">📈 Kardex & Stock</a>
                <a href="pantalla_ajustes_alertas_inventario.html" class="nav-link block px-3 py-1 rounded-md text-sm text-indigo-200 hover:bg-indigo-800 hover:text-white" data-path="pantalla_ajustes_alertas_inventario.html">⚠️ Ajustes & Alertas</a>
            </div>

            <div class="pt-2 border-t border-indigo-800">
                <div class="text-xs font-semibold text-purple-300 uppercase tracking-wider px-3 mb-1">📊 Contabilidad</div>
                <a href="pantalla_plan_cuentas.html" class="nav-link block px-3 py-1 rounded-md text-sm text-indigo-200 hover:bg-indigo-800 hover:text-white" data-path="pantalla_plan_cuentas.html">📑 Plan de Cuentas</a>
                <a href="pantalla_libro_diario.html" class="nav-link block px-3 py-1 rounded-md text-sm text-indigo-200 hover:bg-indigo-800 hover:text-white" data-path="pantalla_libro_diario.html">📖 Libro Diario & Mayor</a>
                <a href="pantalla_estados_financieros.html" class="nav-link block px-3 py-1 rounded-md text-sm text-indigo-200 hover:bg-indigo-800 hover:text-white" data-path="pantalla_estados_financieros.html">📈 Estados Financieros</a>
            </div>
            
            <div class="pt-2 border-t border-indigo-800">
                <div class="text-xs font-semibold text-orange-300 uppercase tracking-wider px-3 mb-1">⚙️ Admin</div>
                <a href="pantalla_admin_usuarios.html" class="nav-link block px-3 py-1 rounded-md text-sm text-indigo-200 hover:bg-indigo-800 hover:text-white" data-path="pantalla_admin_usuarios.html">👥 Gestión Usuarios</a>
                <a href="pantalla_configuracion_integracion.html" class="nav-link block px-3 py-1 rounded-md text-sm text-indigo-200 hover:bg-indigo-800 hover:text-white" data-path="pantalla_configuracion_integracion.html">🔌 Configuración & Integración</a>
            </div>
            
            <div class="pt-2 border-t border-indigo-800">
                <button onclick="logout()" class="w-full text-left px-3 py-1 rounded-md text-sm font-bold text-red-300 hover:bg-red-800 hover:text-white transition-colors">🚪 Salir</button>
            </div>
        </div>
    </nav>
    <script>
        document.addEventListener('DOMContentLoaded', () => {
            const btn = document.getElementById('mobile-menu-btn');
            const menu = document.getElementById('mobile-menu');
            const iconMenu = document.getElementById('icon-menu');
            const iconClose = document.getElementById('icon-close');
            if(btn && menu) {
                btn.addEventListener('click', () => {
                    menu.classList.toggle('hidden');
                    if(iconMenu) iconMenu.classList.toggle('hidden');
                    if(iconClose) iconClose.classList.toggle('hidden');
                });
            }
            const currentPath = window.location.pathname.split('/').pop() || 'CURRENT_FILE_PLACEHOLDER';
            document.querySelectorAll('.nav-link').forEach(link => {
                if(link.getAttribute('data-path') === currentPath) {
                    link.classList.remove('text-indigo-200', 'text-slate-700', 'text-indigo-100');
                    link.classList.add('bg-indigo-700', 'text-white', 'font-bold');
                }
            });
        });
    </script>
    <!-- FIN NAVBAR -->"""

auth_script = """    <script>
        if (localStorage.getItem('isAuthenticated') !== 'true') {
            window.location.href = 'login.html';
        }
        function logout() {
            localStorage.removeItem('isAuthenticated');
            window.location.href = 'login.html';
        }
    </script>
"""

html_files = [f for f in os.listdir(dir_path) if f.endswith(".html") and f != "login.html"]

for file in html_files:
    file_path = os.path.join(dir_path, file)
    with open(file_path, "r", encoding="utf-8") as f:
        content = f.read()
    
    # 1. Reemplazar Navbar
    # Encuentra el inicio de NAVBAR RESPONSIVA UNIFICADA
    start_idx = content.find("<!-- NAVBAR RESPONSIVA UNIFICADA -->")
    
    # Encuentra el final del navbar: puede ser <!-- FIN NAVBAR -->, o el tag <main, o <div id="root"
    end_idx_main = content.find("<main", start_idx)
    end_idx_root = content.find('<div id="root"', start_idx)
    end_idx_app = content.find('<div id="app"', start_idx)
    end_idx_fin = content.find("<!-- FIN NAVBAR -->", start_idx)
    
    # Encontrar el minimo positivo de los posibles finales
    possible_ends = [idx for idx in [end_idx_main, end_idx_root, end_idx_app] if idx != -1]
    
    if start_idx != -1 and possible_ends:
        end_idx = min(possible_ends)
        # Si tiene <!-- FIN NAVBAR --> antes del main/root, usémoslo + longitud
        if end_idx_fin != -1 and end_idx_fin < end_idx:
            end_idx = end_idx_fin + len("<!-- FIN NAVBAR -->")
        
        old_nav = content[start_idx:end_idx]
        file_new_nav = new_nav.replace("CURRENT_FILE_PLACEHOLDER", file)
        content = content.replace(old_nav, file_new_nav + "\\n\\n    ")
    
    # 2. Agregar Auth Script
    if "localStorage.getItem('isAuthenticated')" not in content:
        content = content.replace("</head>", auth_script + "</head>")
        
    # 3. Arreglar React JSX
    babel_start = content.find('<script type="text/babel">')
    if babel_start != -1:
        babel_part = content[babel_start:]
        # Fix HTML comments inside JSX
        babel_part = re.sub(r'<!--(.*?)-->', r'{/*\1*/}', babel_part)
        # Fix class= to className= (only inside JSX tags, rough regex)
        # We can just replace class=" with className=" in the babel part safely for these simple files
        babel_part = babel_part.replace('class="', 'className="')
        
        content = content[:babel_start] + babel_part

    # También actualizar el dashboard de index_erp.html
    if file == "index_erp.html":
        # Separar "Contabilidad & Admin" en los recuadros principales del dashboard
        content = content.replace("Contabilidad & Admin", "Contabilidad")
        content = content.replace("📦 Inventario (Cristobal)", "📦 Inventario (Adan)")
        content = content.replace("Responsable: Cristobal", "Responsable: Adan", 1) # First one is Inventario
        # Admin is missing from index? Let's just fix the responsible in Inventario for now.
    
    with open(file_path, "w", encoding="utf-8") as f:
        f.write(content)

print("Todo procesado correctamente.")
