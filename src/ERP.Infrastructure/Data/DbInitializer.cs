using System.Security.Cryptography;
using System.Text;
using ERP.Domain.Entities.Contabilidad;
using ERP.Domain.Entities.Maestros;
using ERP.Domain.Entities.Seguridad;
using ERP.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ErpDbContext context)
    {
        // 1. Roles
        if (!await context.Roles.AnyAsync())
        {
            var roles = new List<Rol>
            {
                new() { Id = (int)RolUsuario.Administrador, Nombre = "Administrador", Descripcion = "Acceso total a todos los módulos" },
                new() { Id = (int)RolUsuario.Comprador, Nombre = "Comprador", Descripcion = "Gestión de compras, proveedores y órdenes (Adan)" },
                new() { Id = (int)RolUsuario.Vendedor, Nombre = "Vendedor", Descripcion = "Gestión de clientes, ventas y facturación (Maria)" },
                new() { Id = (int)RolUsuario.Bodeguero, Nombre = "Bodeguero", Descripcion = "Control de inventario, kardex y stock (Cristobal)" },
                new() { Id = (int)RolUsuario.Contador, Nombre = "Contador", Descripcion = "Gestión contable, asientos y balances (Cristobal)" }
            };

            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
        }

        // 2. Permisos
        if (!await context.Permisos.AnyAsync())
        {
            var permisos = new List<Permiso>
            {
                new() { Codigo = "compras.proveedores.leer", Modulo = "Compras", Descripcion = "Ver proveedores" },
                new() { Codigo = "compras.proveedores.escribir", Modulo = "Compras", Descripcion = "Crear/Editar proveedores" },
                new() { Codigo = "compras.ordenes.leer", Modulo = "Compras", Descripcion = "Ver órdenes de compra" },
                new() { Codigo = "compras.ordenes.escribir", Modulo = "Compras", Descripcion = "Crear/Editar órdenes de compra" },
                new() { Codigo = "compras.recepciones.crear", Modulo = "Compras", Descripcion = "Registrar recepciones de mercadería" },
                new() { Codigo = "ventas.clientes.leer", Modulo = "Ventas", Descripcion = "Ver clientes" },
                new() { Codigo = "ventas.documentos.crear", Modulo = "Ventas", Descripcion = "Emitir ventas/facturas" },
                new() { Codigo = "ventas.documentos.anular", Modulo = "Ventas", Descripcion = "Anular ventas emitidas" },
                new() { Codigo = "inventario.productos.escribir", Modulo = "Inventario", Descripcion = "Crear/Editar productos" },
                new() { Codigo = "inventario.stock.leer", Modulo = "Inventario", Descripcion = "Consultar stock y kardex" },
                new() { Codigo = "inventario.ajustes.crear", Modulo = "Inventario", Descripcion = "Registrar ajustes de inventario" },
                new() { Codigo = "contabilidad.asientos.leer", Modulo = "Contabilidad", Descripcion = "Ver asientos contables y libros" },
                new() { Codigo = "contabilidad.asientos.crear", Modulo = "Contabilidad", Descripcion = "Crear asientos contables" },
                new() { Codigo = "contabilidad.reportes.leer", Modulo = "Contabilidad", Descripcion = "Ver balances y estado de resultados" },
                new() { Codigo = "usuarios.administrar", Modulo = "Seguridad", Descripcion = "Administrar usuarios y roles" }
            };

            await context.Permisos.AddRangeAsync(permisos);
            await context.SaveChangesAsync();
        }

        // 3. Usuario Administrador por defecto
        if (!await context.Usuarios.AnyAsync())
        {
            var adminUser = new Usuario
            {
                NombreCompleto = "Administrador ERP",
                Email = "admin@erp.com",
                Rut = "11.111.111-1",
                PasswordHash = HashPassword("Admin123!"),
                RolId = (int)RolUsuario.Administrador,
                Activo = true,
                CreadoPor = "System"
            };

            await context.Usuarios.AddAsync(adminUser);
            await context.SaveChangesAsync();
        }

        // 4. Plan de Cuentas Contables inicial
        if (!await context.CuentasContables.AnyAsync())
        {
            var cuentas = new List<CuentaContable>
            {
                new() { Codigo = "1.1.01.01", Nombre = "Caja General", Tipo = TipoCuentaContable.Activo, Naturaleza = NaturalezaCuenta.Deudora, Nivel = 4, PermiteMovimiento = true },
                new() { Codigo = "1.1.01.02", Nombre = "Banco Principal", Tipo = TipoCuentaContable.Activo, Naturaleza = NaturalezaCuenta.Deudora, Nivel = 4, PermiteMovimiento = true },
                new() { Codigo = "1.1.02.01", Nombre = "Clientes Nacionales (Cuentas por Cobrar)", Tipo = TipoCuentaContable.Activo, Naturaleza = NaturalezaCuenta.Deudora, Nivel = 4, PermiteMovimiento = true },
                new() { Codigo = "1.1.03.01", Nombre = "Inventario de Mercaderías", Tipo = TipoCuentaContable.Activo, Naturaleza = NaturalezaCuenta.Deudora, Nivel = 4, PermiteMovimiento = true },
                new() { Codigo = "1.1.04.01", Nombre = "IVA Crédito Fiscal (Compras)", Tipo = TipoCuentaContable.Activo, Naturaleza = NaturalezaCuenta.Deudora, Nivel = 4, PermiteMovimiento = true },
                new() { Codigo = "2.1.01.01", Nombre = "Proveedores Nacionales (Cuentas por Pagar)", Tipo = TipoCuentaContable.Pasivo, Naturaleza = NaturalezaCuenta.Acreedora, Nivel = 4, PermiteMovimiento = true },
                new() { Codigo = "2.1.02.01", Nombre = "IVA Débito Fiscal (Ventas)", Tipo = TipoCuentaContable.Pasivo, Naturaleza = NaturalezaCuenta.Acreedora, Nivel = 4, PermiteMovimiento = true },
                new() { Codigo = "3.1.01.01", Nombre = "Capital Social", Tipo = TipoCuentaContable.Patrimonio, Naturaleza = NaturalezaCuenta.Acreedora, Nivel = 4, PermiteMovimiento = true },
                new() { Codigo = "4.1.01.01", Nombre = "Ingresos por Ventas", Tipo = TipoCuentaContable.Ingreso, Naturaleza = NaturalezaCuenta.Acreedora, Nivel = 4, PermiteMovimiento = true },
                new() { Codigo = "5.1.01.01", Nombre = "Costo de Ventas", Tipo = TipoCuentaContable.Costo, Naturaleza = NaturalezaCuenta.Deudora, Nivel = 4, PermiteMovimiento = true },
                new() { Codigo = "6.1.01.01", Nombre = "Gastos Operacionales / Mermas", Tipo = TipoCuentaContable.Gasto, Naturaleza = NaturalezaCuenta.Deudora, Nivel = 4, PermiteMovimiento = true }
            };

            await context.CuentasContables.AddRangeAsync(cuentas);
            await context.SaveChangesAsync();
        }
    }

    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    public static bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }
}
