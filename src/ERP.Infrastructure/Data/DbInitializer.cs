// Módulo: Transversal (datos iniciales) — Responsable: Cristóbal
using ERP.Application.Common;
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
    public static async Task SeedAsync(ErpDbContext context, string? adminPassword = null, string? cajeroPassword = null)
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
    }
}
