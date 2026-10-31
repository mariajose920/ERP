// Módulo: Inventario — reglas de unidad de medida y normalización de nombres de producto.
namespace ERP.Application.Common;

public static class UnidadesMedidaConst
{
    /// <summary>Unidades que admiten cantidades fraccionarias (peso, volumen, longitud).</summary>
    private static readonly HashSet<string> Decimales = new(StringComparer.OrdinalIgnoreCase) { "KG", "LT", "MT" };

    public static bool AdmiteDecimales(string? unidad) => unidad != null && Decimales.Contains(unidad.Trim());

    /// <summary>Exige cantidad entera cuando la unidad es de conteo (UN, CJ...).</summary>
    public static void ValidarCantidad(string? unidad, decimal cantidad, string campo)
    {
        if (!AdmiteDecimales(unidad) && cantidad != Math.Truncate(cantidad))
            throw new InvalidOperationException($"{campo} debe ser un número entero: la unidad {unidad ?? "UN"} no admite decimales.");
    }

    /// <summary>Nombre en un único formato: sin espacios sobrantes y en MAYÚSCULAS.</summary>
    public static string NormalizarNombre(string nombre) =>
        string.Join(' ', nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)).ToUpperInvariant();
}
