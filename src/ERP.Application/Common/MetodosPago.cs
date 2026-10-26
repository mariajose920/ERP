// Módulo: Ventas — Responsable: María
namespace ERP.Application.Common;

/// <summary>
/// Métodos de pago válidos (M5). Se usan constantes en lugar de un enum para conservar la columna
/// string existente en VentasClientes/CobrosClientes sin migración ni conversión de datos.
/// </summary>
public static class MetodosPago
{
    public const string Efectivo = "Efectivo";
    public const string Transferencia = "Transferencia";
    public const string Tarjeta = "Tarjeta";
    public const string Credito = "Credito";

    private static readonly string[] Todos = { Efectivo, Transferencia, Tarjeta, Credito };

    /// <summary>
    /// Devuelve el valor canónico (acepta mayúsculas/minúsculas y "Crédito" con tilde).
    /// Lanza <see cref="InvalidOperationException"/> si el método no es válido.
    /// </summary>
    public static string Normalizar(string? metodo)
    {
        var limpio = (metodo ?? string.Empty).Trim().Replace("é", "e").Replace("É", "E");
        return Todos.FirstOrDefault(m => m.Equals(limpio, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException(
                $"Método de pago inválido: '{metodo}'. Valores permitidos: {string.Join(", ", Todos)}.");
    }

    /// <summary>True si la venta queda pagada al emitirse (todo menos Crédito).</summary>
    public static bool EsContado(string metodoNormalizado) => metodoNormalizado != Credito;

    /// <summary>True si el dinero entra a Caja (efectivo); el resto entra a Banco.</summary>
    public static bool EntraACaja(string metodoNormalizado) => metodoNormalizado == Efectivo;
}
