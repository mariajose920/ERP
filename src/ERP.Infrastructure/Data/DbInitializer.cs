// Módulo: Transversal (datos iniciales) — Responsable: Cristóbal
using ERP.Application.Common;
using ERP.Domain.Entities.Inventario;
using ERP.Domain.Entities.Maestros;
using ERP.Domain.Entities.Seguridad;
using ERP.Domain.Enums;
using ERP.Infrastructure.Services.Seguridad;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Data;

/// <summary>
/// Carga de datos iniciales (idempotente): roles, permisos y su asignación por rol, usuarios
/// iniciales y plan de cuentas base. Solo inserta lo que falta; nunca borra ni sobrescribe.
/// </summary>
public static class DbInitializer
{
    /// <summary>Correo del administrador opcional creado cuando se configura Seed:AdminPassword.</summary>
    public const string EmailAdminPorDefecto = "admin@erp.com";

    /// <summary>Correo del cajero creado solo si no existe el usuario CAJA1.</summary>
    public const string EmailCajeroPorDefecto = "cajero@erp.com";

    /// <summary>Contraseña inicial de CAJA1/CAJA2 (seed de desarrollo).</summary>
    public const string PasswordCajerosSeed = "CAJA123";

    /// <summary>
    /// Usuarios de demostración de los perfiles de gestión (Informe, 8.1.1 Actores): rol → correo.
    /// Solo se crean si se configura "Seed:PerfilesPassword" (desarrollo).
    /// </summary>
    public static readonly IReadOnlyList<(string Rol, string Email, string Nombre, string Rut)> UsuariosPerfiles = new[]
    {
        ("Vendedor", "vendedor@erp.com", "Vendedor ERP", "33.333.333-3"),
        ("Comprador", "comprador@erp.com", "Comprador ERP", "44.444.444-4"),
        ("Bodeguero", "bodeguero@erp.com", "Bodeguero ERP", "55.555.555-5"),
        ("Contador", "contador@erp.com", "Contador ERP", "66.666.666-6")
    };

    /// <summary>Ejecuta el seed.</summary>
    /// <param name="context">Contexto de base de datos.</param>
    /// <param name="adminPassword">
    /// Contraseña para el usuario admin@erp.com (configuración "Seed:AdminPassword"). Si es null o vacía,
    /// ese usuario no se crea.
    /// </param>
    /// <param name="cajeroPassword">
    /// Contraseña de cajero@erp.com ("Seed:CajeroPassword"). Solo se usa si no existe CAJA1
    /// (que se reutiliza como cajero).
    /// </param>
    /// <param name="perfilesPassword">
    /// Contraseña de los usuarios de Vendedor, Comprador, Bodeguero y Contador ("Seed:PerfilesPassword").
    /// Si es null o vacía, esos usuarios no se crean.
    /// </param>
    public static async Task SeedAsync(ErpDbContext context, string? adminPassword = null, string? cajeroPassword = null, string? perfilesPassword = null)
    {
        // 1. Roles
        if (!await context.Roles.AnyAsync())
        {
            var roles = new List<Rol>
            {
                new() { Nombre = "Administrador", Descripcion = "Acceso total a todos los módulos" },
                new() { Nombre = "Comprador", Descripcion = "Gestión de compras, proveedores y órdenes (Adán)" },
                new() { Nombre = "Vendedor", Descripcion = "Gestión de clientes, ventas y facturación (María)" },
                new() { Nombre = "Bodeguero", Descripcion = "Control de inventario, kardex y stock (Adán)" },
                new() { Nombre = "Contador", Descripcion = "Gestión contable, asientos y balances (Cristóbal)" }
            };

            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
        }

        // 2. Permisos: se agregan los que falten (K5 agregó códigos nuevos).
        var existentes = await context.Permisos.Select(p => p.Codigo).ToListAsync();
        var nuevos = PermisosConst.Todos
            .Where(p => !existentes.Contains(p.Codigo))
            .Select(p => new Permiso { Codigo = p.Codigo, Modulo = p.Modulo, Descripcion = p.Descripcion, CreadoPor = "System" })
            .ToList();
        if (nuevos.Count > 0)
        {
            await context.Permisos.AddRangeAsync(nuevos);
            await context.SaveChangesAsync();
        }

        // 2b. Rol Cajero (perfil de caja restringido).
        if (!await context.Roles.AnyAsync(r => r.Nombre == PermisosConst.RolCajero))
        {
            await context.Roles.AddAsync(new Rol
            {
                Nombre = PermisosConst.RolCajero,
                Descripcion = "Caja: POS, clientes básicos, abonos y ventas propias del día (María)",
                CreadoPor = "System"
            });
            await context.SaveChangesAsync();
        }

        // 3. Asignación de permisos por rol. Es idempotente: solo AGREGA las asignaciones que falten
        //    según estas reglas (no quita las que se hayan dado manualmente). El Cajero, en cambio,
        //    queda con EXACTAMENTE PermisosConst.PermisosCajero (se quitan los sobrantes).
        {
            var permisos = await context.Permisos.ToListAsync();
            var roles = await context.Roles.ToListAsync();
            var actuales = await context.RolPermisos.ToListAsync();

            void Asignar(string rol, Func<string, bool> filtro)
            {
                var r = roles.FirstOrDefault(x => x.Nombre == rol);
                if (r == null) return;
                foreach (var p in permisos.Where(p => filtro(p.Codigo)))
                    if (!actuales.Any(a => a.RolId == r.Id && a.PermisoId == p.Id))
                        context.RolPermisos.Add(new RolPermiso { RolId = r.Id, PermisoId = p.Id });
            }

            Asignar(PermisosConst.RolAdministrador, _ => true);
            Asignar("Comprador", c => c.StartsWith("compras.") || c == PermisosConst.InventarioStockLeer);
            Asignar("Vendedor", c => c.StartsWith("ventas.") || c == PermisosConst.InventarioStockLeer);
            Asignar("Bodeguero", c => c.StartsWith("inventario.") || c == PermisosConst.ComprasOrdenesLeer || c == PermisosConst.ComprasRecepcionesCrear);
            Asignar("Contador", c => c.StartsWith("contabilidad.") || c == PermisosConst.VentasDocumentosLeer || c == PermisosConst.ComprasOrdenesLeer);
            Asignar(PermisosConst.RolCajero, c => PermisosConst.PermisosCajero.Contains(c));

            var cajero = roles.First(r => r.Nombre == PermisosConst.RolCajero);
            var idsCajero = permisos.Where(p => PermisosConst.PermisosCajero.Contains(p.Codigo)).Select(p => p.Id).ToHashSet();
            context.RolPermisos.RemoveRange(actuales.Where(a => a.RolId == cajero.Id && !idsCajero.Contains(a.PermisoId)));

            await context.SaveChangesAsync();
        }

        // 4. Usuarios iniciales (ADMIN, CAJA1, etc.). Contraseñas con PBKDF2 (K7).
        if (!await context.Usuarios.AnyAsync(u => u.Email == "ADMIN"))
        {
            var adminUser = new Usuario { NombreCompleto = "Administrador ERP", Email = "ADMIN", Rut = "11.111.111-1", PasswordHash = HashContrasenas.Hashear("ADMIN123"), RolId = (int)RolUsuario.Administrador, Activo = true, CreadoPor = "System" };
            var admin1User = new Usuario { NombreCompleto = "Administrador 1", Email = "ADMIN1", Rut = "11.111.111-2", PasswordHash = HashContrasenas.Hashear("ADMIN123"), RolId = (int)RolUsuario.Administrador, Activo = true, CreadoPor = "System" };
            var caja1User = new Usuario { NombreCompleto = "Cajero 1", Email = "CAJA1", Rut = "22.222.222-1", PasswordHash = HashContrasenas.Hashear(PasswordCajerosSeed), RolId = (int)RolUsuario.Vendedor, Activo = true, CreadoPor = "System" };
            var caja2User = new Usuario { NombreCompleto = "Cajero 2", Email = "CAJA2", Rut = "22.222.222-2", PasswordHash = HashContrasenas.Hashear(PasswordCajerosSeed), RolId = (int)RolUsuario.Vendedor, Activo = true, CreadoPor = "System" };

            await context.Usuarios.AddRangeAsync(adminUser, admin1User, caja1User, caja2User);
            await context.SaveChangesAsync();
        }

        // 4b. Administrador con correo (el que usa la pantalla de login). Solo si se configuró su contraseña.
        if (!string.IsNullOrWhiteSpace(adminPassword) &&
            !await context.Usuarios.AnyAsync(u => u.Email == EmailAdminPorDefecto))
        {
            await context.Usuarios.AddAsync(new Usuario
            {
                NombreCompleto = "Administrador ERP (correo)",
                Email = EmailAdminPorDefecto,
                Rut = "11.111.111-3",
                PasswordHash = HashContrasenas.Hashear(adminPassword),
                RolId = (int)RolUsuario.Administrador,
                Activo = true,
                CreadoPor = "System"
            });
            await context.SaveChangesAsync();
        }

        // 4c. Usuario cajero: se reutiliza CAJA1 (pasa al rol Cajero); si no existe, se crea
        //     cajero@erp.com con la contraseña de desarrollo "Seed:CajeroPassword".
        var rolCajeroId = await context.Roles.Where(r => r.Nombre == PermisosConst.RolCajero).Select(r => r.Id).FirstAsync();
        var caja1 = await context.Usuarios.FirstOrDefaultAsync(u => u.Email == "CAJA1");
        if (caja1 != null)
        {
            if (caja1.RolId != rolCajeroId)
            {
                caja1.RolId = rolCajeroId;
                caja1.ModificadoPor = "System";
                caja1.FechaModificacion = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
        }
        else if (!string.IsNullOrWhiteSpace(cajeroPassword) &&
                 !await context.Usuarios.AnyAsync(u => u.Email == EmailCajeroPorDefecto))
        {
            await context.Usuarios.AddAsync(new Usuario
            {
                NombreCompleto = "Cajero ERP",
                Email = EmailCajeroPorDefecto,
                Rut = "22.222.222-3",
                PasswordHash = HashContrasenas.Hashear(cajeroPassword),
                RolId = rolCajeroId,
                Activo = true,
                CreadoPor = "System"
            });
            await context.SaveChangesAsync();
        }

        // 4d. Un usuario por cada perfil de gestión (Vendedor, Comprador, Bodeguero, Contador), para que
        //     los seis roles del informe puedan probarse desde el login. Solo agrega los que falten.
        if (!string.IsNullOrWhiteSpace(perfilesPassword))
        {
            var rolesPorNombre = await context.Roles.ToDictionaryAsync(r => r.Nombre, r => r.Id);
            var agregados = false;
            foreach (var p in UsuariosPerfiles)
            {
                if (!rolesPorNombre.TryGetValue(p.Rol, out var rolId)) continue;
                if (await context.Usuarios.AnyAsync(u => u.Email == p.Email)) continue;
                await context.Usuarios.AddAsync(new Usuario
                {
                    NombreCompleto = p.Nombre,
                    Email = p.Email,
                    Rut = p.Rut,
                    PasswordHash = HashContrasenas.Hashear(perfilesPassword),
                    RolId = rolId,
                    Activo = true,
                    CreadoPor = "System"
                });
                agregados = true;
            }
            if (agregados) await context.SaveChangesAsync();
        }

        // 5. Plan de cuentas inicial (los códigos están en CuentasContablesConst).
        if (!await context.CuentasContables.AnyAsync())
        {
            var cuentas = new List<CuentaContable>
            {
                new() { Codigo = CuentasContablesConst.Caja, Nombre = "Caja General", Tipo = TipoCuentaContable.Activo, Naturaleza = NaturalezaCuenta.Deudora, Nivel = 4, PermiteMovimiento = true },
                new() { Codigo = CuentasContablesConst.Banco, Nombre = "Banco Principal", Tipo = TipoCuentaContable.Activo, Naturaleza = NaturalezaCuenta.Deudora, Nivel = 4, PermiteMovimiento = true },
                new() { Codigo = CuentasContablesConst.Clientes, Nombre = "Clientes Nacionales (Cuentas por Cobrar)", Tipo = TipoCuentaContable.Activo, Naturaleza = NaturalezaCuenta.Deudora, Nivel = 4, PermiteMovimiento = true },
                new() { Codigo = CuentasContablesConst.Inventario, Nombre = "Inventario de Mercaderías", Tipo = TipoCuentaContable.Activo, Naturaleza = NaturalezaCuenta.Deudora, Nivel = 4, PermiteMovimiento = true },
                new() { Codigo = CuentasContablesConst.IvaCreditoFiscal, Nombre = "IVA Crédito Fiscal (Compras)", Tipo = TipoCuentaContable.Activo, Naturaleza = NaturalezaCuenta.Deudora, Nivel = 4, PermiteMovimiento = true },
                new() { Codigo = CuentasContablesConst.Proveedores, Nombre = "Proveedores Nacionales (Cuentas por Pagar)", Tipo = TipoCuentaContable.Pasivo, Naturaleza = NaturalezaCuenta.Acreedora, Nivel = 4, PermiteMovimiento = true },
                new() { Codigo = CuentasContablesConst.IvaDebitoFiscal, Nombre = "IVA Débito Fiscal (Ventas)", Tipo = TipoCuentaContable.Pasivo, Naturaleza = NaturalezaCuenta.Acreedora, Nivel = 4, PermiteMovimiento = true },
                new() { Codigo = "3.1.01.01", Nombre = "Capital Social", Tipo = TipoCuentaContable.Patrimonio, Naturaleza = NaturalezaCuenta.Acreedora, Nivel = 4, PermiteMovimiento = true },
                new() { Codigo = CuentasContablesConst.IngresosVentas, Nombre = "Ingresos por Ventas", Tipo = TipoCuentaContable.Ingreso, Naturaleza = NaturalezaCuenta.Acreedora, Nivel = 4, PermiteMovimiento = true },
                new() { Codigo = CuentasContablesConst.CostoVentas, Nombre = "Costo de Ventas", Tipo = TipoCuentaContable.Costo, Naturaleza = NaturalezaCuenta.Deudora, Nivel = 4, PermiteMovimiento = true },
                new() { Codigo = CuentasContablesConst.GastosMermas, Nombre = "Gastos Operacionales / Mermas", Tipo = TipoCuentaContable.Gasto, Naturaleza = NaturalezaCuenta.Deudora, Nivel = 4, PermiteMovimiento = true }
            };

            await context.CuentasContables.AddRangeAsync(cuentas);
            await context.SaveChangesAsync();
        }
        // 5b. Cuentas agregadas después del plan inicial (solo si faltan): recepciones por facturar
        //     (la recepción ingresa stock antes de la factura) y diferencias de arqueo de caja.
        {
            var nuevasCuentas = new List<CuentaContable>
            {
                new() { Codigo = CuentasContablesConst.RecepcionesPorFacturar, Nombre = "Mercaderías Recibidas por Facturar", Tipo = TipoCuentaContable.Pasivo, Naturaleza = NaturalezaCuenta.Acreedora, Nivel = 4, PermiteMovimiento = true },
                new() { Codigo = CuentasContablesConst.DiferenciasCaja, Nombre = "Diferencias de Caja (Arqueo)", Tipo = TipoCuentaContable.Gasto, Naturaleza = NaturalezaCuenta.Deudora, Nivel = 4, PermiteMovimiento = true }
            };
            var codigos = await context.CuentasContables.Select(c => c.Codigo).ToListAsync();
            var faltantes = nuevasCuentas.Where(c => !codigos.Contains(c.Codigo)).ToList();
            if (faltantes.Count > 0)
            {
                foreach (var c in faltantes) c.CreadoPor = "System";
                await context.CuentasContables.AddRangeAsync(faltantes);
                await context.SaveChangesAsync();
            }

            // La factura de compra ya no ingresa inventario: su neto salda "Recepciones por facturar".
            var transitoria = await context.CuentasContables.FirstOrDefaultAsync(c => c.Codigo == CuentasContablesConst.RecepcionesPorFacturar);
            var inventario = await context.CuentasContables.FirstOrDefaultAsync(c => c.Codigo == CuentasContablesConst.Inventario);
            if (transitoria != null && inventario != null)
            {
                var filaNeto = await context.MatrizIntegracionContable
                    .FirstOrDefaultAsync(m => m.ModuloOrigen == "Compras" && m.TipoOperacion == "FacturaCompraNeto" && m.CuentaDebeId == inventario.Id);
                if (filaNeto != null)
                {
                    filaNeto.CuentaDebeId = transitoria.Id;
                    filaNeto.Descripcion = "Factura de compra: salda la recepción por facturar (neto) contra cuenta por pagar";
                    filaNeto.ModificadoPor = "System";
                    filaNeto.FechaModificacion = DateTime.UtcNow;
                    await context.SaveChangesAsync();
                }
            }
        }

        // 6. Matriz de integración contable: se siembra desde las cuentas fijas (MatrizIntegracionSemilla),
        //    solo con las operaciones que falten y cuyas cuentas existan; nunca pisa lo ya configurado.
        {
            var cuentasPorCodigo = await context.CuentasContables.ToDictionaryAsync(c => c.Codigo, c => c.Id);
            var existentesMatriz = await context.MatrizIntegracionContable
                .Select(m => m.ModuloOrigen + "|" + m.TipoOperacion).ToListAsync();
            var agregadas = false;
            foreach (var f in MatrizIntegracionSemilla.Filas)
            {
                if (existentesMatriz.Contains(f.Modulo + "|" + f.Operacion)) continue;
                if (!cuentasPorCodigo.TryGetValue(f.Debe, out var debeId) || !cuentasPorCodigo.TryGetValue(f.Haber, out var haberId)) continue;
                context.MatrizIntegracionContable.Add(new ERP.Domain.Entities.Contabilidad.MatrizIntegracionContable
                {
                    ModuloOrigen = f.Modulo, TipoOperacion = f.Operacion, CuentaDebeId = debeId, CuentaHaberId = haberId,
                    Descripcion = f.Descripcion, CreadoPor = "System"
                });
                agregadas = true;
            }
            if (agregadas) await context.SaveChangesAsync();
        }

        // 7. Proveedores iniciales (Módulo de Compras — Adán).
        var proveedoresSemilla = new List<Proveedor>
        {
            new()
            {
                Rut = "75892574-9",
                RazonSocial = "COMERCIALIZADORA ABARROTES",
                Giro = "COMERCIALIZADORA",
                Email = "comercializadoraabarrotes@gmail.com",
                CondicionesPago = "30 días",
                Activo = true,
                CreadoPor = "System"
            },
            new()
            {
                Rut = "76543210-3",
                RazonSocial = "FERRO FIX",
                Giro = "ARTICULOS DE FERRETERIA",
                Email = "ferrofixltda@gmail.com",
                CondicionesPago = "30 días",
                Activo = true,
                CreadoPor = "System"
            },
            new()
            {
                Rut = "76543212-K",
                RazonSocial = "MEDI-CORE",
                Giro = "MATERIAL Y EQUIPAMIENTO MEDICO",
                Direccion = "Av siempreviva 742",
                Email = "medi-core@gmail.com",
                CondicionesPago = "30 días",
                Activo = true,
                CreadoPor = "System"
            },
            new()
            {
                Rut = "77123456-9",
                RazonSocial = "DISTRIBUIDORA TECNOLOGICA SUR",
                Giro = "SUMINISTROS Y TECNOLOGIA",
                Direccion = "Av. Los Robles 1020, Concepción",
                Email = "contacto@tecnologicasur.cl",
                CondicionesPago = "30 días",
                Activo = true,
                CreadoPor = "System"
            }
        };

        foreach (var prov in proveedoresSemilla)
        {
            if (!await context.Proveedores.AnyAsync(p => p.Rut == prov.Rut))
            {
                await context.Proveedores.AddAsync(prov);
            }
        }
        await context.SaveChangesAsync();

        // 8. Categorías iniciales (Módulo de Inventario — Adán).
        var categoriasSemilla = new List<Categoria>
        {
            new() { Codigo = "CAT-001", Nombre = "ABARROTE", Descripcion = "Artículos de abarrotes y alimentos", Activo = true, CreadoPor = "System" },
            new() { Codigo = "CAT-002", Nombre = "FERRETERIA", Descripcion = "Artículos de ferretería y herramientas", Activo = true, CreadoPor = "System" },
            new() { Codigo = "CAT-003", Nombre = "EQUIPAMIENTO MEDICO", Descripcion = "Material y equipamiento médico", Activo = true, CreadoPor = "System" }
        };

        foreach (var cat in categoriasSemilla)
        {
            if (!await context.Categorias.AnyAsync(c => c.Nombre == cat.Nombre))
            {
                await context.Categorias.AddAsync(cat);
            }
        }
        await context.SaveChangesAsync();

        var categoriasMap = await context.Categorias.AsNoTracking()
            .Where(c => c.Activo)
            .ToDictionaryAsync(c => c.Nombre, c => c.Id);

        // 9. Productos iniciales de inventario con stock y kardex (Módulo de Inventario — Adán).
        var productosSemilla = new List<(string Sku, string Nombre, string Categoria, decimal PrecioVenta, decimal Cpp, decimal Stock, decimal Reorden, decimal StockMinimo)>
        {
            ("89765", "ACEITE", "ABARROTE", 2200m, 800m, 12m, 10m, 5m),
            ("54641", "ARROZ", "ABARROTE", 1770m, 500m, 10m, 8m, 5m),
            ("7458", "BISTURI", "EQUIPAMIENTO MEDICO", 300m, 50m, 15m, 10m, 5m),
            ("95682", "CABLES FLEXIBLE", "FERRETERIA", 1770m, 600m, 26m, 26m, 10m),
            ("785168", "CLAVOS", "FERRETERIA", 1850m, 500m, 112m, 90m, 50m),
            ("4454", "GASA ESTERIL", "EQUIPAMIENTO MEDICO", 1501m, 800m, 9m, 10m, 5m),
            ("7845", "GUANTES DE LATEX", "EQUIPAMIENTO MEDICO", 4150m, 1000m, 20m, 12m, 10m),
            ("154228", "PAN", "ABARROTE", 1178m, 500m, 25m, 7m, 5m),
            ("165187", "PEGAMENTO PVC", "FERRETERIA", 4760m, 1250m, 7m, 10m, 5m),
            ("54654", "TALLARINES", "ABARROTE", 1785m, 750m, 29m, 22m, 10m),
            ("8761", "TERMOMETRO PRISMATICO", "EQUIPAMIENTO MEDICO", 1501m, 750m, 32m, 18m, 10m),
            ("784516", "TORNILLOS", "FERRETERIA", 1904m, 745m, 40m, 55m, 20m)
        };

        foreach (var item in productosSemilla)
        {
            var prod = await context.Productos.FirstOrDefaultAsync(p => p.Sku == item.Sku);
            if (prod == null)
            {
                if (!categoriasMap.TryGetValue(item.Categoria, out var catId))
                    continue;

                prod = new Producto
                {
                    Sku = item.Sku,
                    Nombre = item.Nombre,
                    CategoriaId = catId,
                    UnidadMedida = "UN",
                    PrecioVenta = item.PrecioVenta,
                    CostoBase = item.Cpp,
                    CostoPromedioPonderado = item.Cpp,
                    StockActual = item.Stock,
                    PuntoReorden = item.Reorden,
                    StockMinimo = item.StockMinimo,
                    Activo = true,
                    CreadoPor = "System"
                };

                await context.Productos.AddAsync(prod);
                await context.SaveChangesAsync();
            }

            // Movimiento inicial de kardex para reflejar stock y CPP en el historial
            if (prod.StockActual > 0 && !await context.MovimientosKardex.AnyAsync(k => k.ProductoId == prod.Id))
            {
                var ajuste = new AjusteInventario
                {
                    NumeroAjuste = $"AJU-INI-{prod.Sku}",
                    Fecha = DateTime.UtcNow,
                    ProductoId = prod.Id,
                    CantidadAnterior = 0,
                    CantidadAjuste = prod.StockActual,
                    CantidadNueva = prod.StockActual,
                    CostoUnitario = prod.CostoPromedioPonderado,
                    Motivo = "Stock inicial",
                    CreadoPor = "System"
                };
                await context.AjustesInventario.AddAsync(ajuste);
                await context.SaveChangesAsync();

                var kardex = new MovimientoKardex
                {
                    ProductoId = prod.Id,
                    Fecha = DateTime.UtcNow,
                    TipoMovimiento = TipoMovimientoKardex.AjusteEntrada,
                    DocumentoTipo = "AjusteInventario",
                    DocumentoNumero = ajuste.NumeroAjuste,
                    DocumentoId = ajuste.Id,
                    CantidadEntrada = prod.StockActual,
                    CostoUnitarioEntrada = prod.CostoPromedioPonderado,
                    CantidadSalida = 0,
                    CostoUnitarioSalida = 0,
                    SaldoCantidad = prod.StockActual,
                    CostoPromedioPonderadoResultante = prod.CostoPromedioPonderado,
                    SaldoValorizado = prod.StockActual * prod.CostoPromedioPonderado,
                    Glosa = "Stock inicial",
                    CreadoPor = "System"
                };
                await context.MovimientosKardex.AddAsync(kardex);
                await context.SaveChangesAsync();
            }
        }
    }
}
