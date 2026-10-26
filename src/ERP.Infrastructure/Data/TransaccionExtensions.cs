// Módulo: Transversal (Integración) — Responsable: Cristóbal
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Data;

/// <summary>Ayudas para ejecutar operaciones de negocio dentro de una transacción.</summary>
public static class TransaccionExtensions
{
    /// <summary>
    /// Ejecuta <paramref name="operacion"/> en una transacción. Si ya hay una transacción activa
    /// (llamada anidada entre servicios), se reutiliza y quien la abrió hace el commit.
    /// </summary>
    public static async Task<T> EnTransaccionAsync<T>(this ErpDbContext context, Func<Task<T>> operacion)
    {
        if (context.Database.CurrentTransaction != null)
        {
            return await operacion();
        }

        await using var tx = await context.Database.BeginTransactionAsync();
        try
        {
            var resultado = await operacion();
            await tx.CommitAsync();
            return resultado;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}
