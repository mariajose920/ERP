// Módulo: Compras e Inventario (maestros) — Responsable: Adán
using ERP.Domain.Common;

namespace ERP.Domain.Entities.Maestros;

/// <summary>Categoría de productos.</summary>
public class Categoria : BaseEntity
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}

/// <summary>Producto del catálogo, con stock y costo promedio ponderado (CPP).</summary>
public class Producto : BaseEntity
{
    public string Sku { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }
    public string UnidadMedida { get; set; } = "UN"; // UN, KG, LT, etc.
    public decimal PrecioVenta { get; set; }
    public decimal CostoBase { get; set; }
    public decimal CostoPromedioPonderado { get; set; } // CPP vigente
    public decimal StockActual { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal PuntoReorden { get; set; }

    // Cuentas contables vinculadas
    public int? CuentaInventarioId { get; set; }
    public CuentaContable? CuentaInventario { get; set; }
    public int? CuentaVentaId { get; set; }
    public CuentaContable? CuentaVenta { get; set; }
    public int? CuentaCostoVentaId { get; set; }
    public CuentaContable? CuentaCostoVenta { get; set; }
}

/// <summary>Proveedor de mercadería (la eliminación es lógica: Activo = false).</summary>
public class Proveedor : BaseEntity
{
    public string Rut { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? Giro { get; set; }
    public string? Contacto { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string Email { get; set; } = string.Empty;
    public string CondicionesPago { get; set; } = "Contado"; // Contado, 30 días, 60 días
}
