using ERP.Domain.Common;
using ERP.Domain.Entities.Maestros;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities.Inventario;

public class MovimientoKardex : BaseEntity
{
    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public TipoMovimientoKardex TipoMovimiento { get; set; }
    public string DocumentoTipo { get; set; } = string.Empty; // "FacturaCompra", "FacturaVenta", "AjusteInventario"
    public string DocumentoNumero { get; set; } = string.Empty;
    public int? DocumentoId { get; set; }

    // Cantidades
    public decimal CantidadEntrada { get; set; }
    public decimal CostoUnitarioEntrada { get; set; }
    public decimal CantidadSalida { get; set; }
    public decimal CostoUnitarioSalida { get; set; }

    // Saldo tras el movimiento
    public decimal SaldoCantidad { get; set; }
    public decimal CostoPromedioPonderadoResultante { get; set; } // Nuevo CPP
    public decimal SaldoValorizado { get; set; } // SaldoCantidad * Nuevo CPP

    public string? Glosa { get; set; }
}

public class AjusteInventario : BaseEntity
{
    public string NumeroAjuste { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }
    public decimal CantidadAnterior { get; set; }
    public decimal CantidadAjuste { get; set; } // Positivo (ingreso) o Negativo (merma/pérdida)
    public decimal CantidadNueva { get; set; }
    public decimal CostoUnitario { get; set; }
    public string Motivo { get; set; } = string.Empty; // "Merma", "Inventario Inicial", "Pérdida", "Corrección"
    public int? AsientoContableId { get; set; }
}
