using ERP.Domain.Common;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities.Maestros;

public class Categoria : BaseEntity
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}

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
    public decimal CostoPromedioPonderado { get; set; } // CPP
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

public class Cliente : BaseEntity
{
    public string Rut { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? Giro { get; set; }
    public string? Contacto { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string Email { get; set; } = string.Empty;
    public decimal LimiteCredito { get; set; }
    public decimal SaldoPendiente { get; set; }
}

public class CuentaContable : BaseEntity
{
    public string Codigo { get; set; } = string.Empty; // e.g. "1.1.01.01"
    public string Nombre { get; set; } = string.Empty;
    public TipoCuentaContable Tipo { get; set; }
    public NaturalezaCuenta Naturaleza { get; set; }
    public int Nivel { get; set; } = 1;
    public int? CuentaPadreId { get; set; }
    public CuentaContable? CuentaPadre { get; set; }
    public bool PermiteMovimiento { get; set; } = true;
    public decimal SaldoActual { get; set; }
}
