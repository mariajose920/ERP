// Módulo: Ventas (maestros) — Responsable: María
using ERP.Domain.Common;

namespace ERP.Domain.Entities.Maestros;

/// <summary>
/// Cliente. <see cref="SaldoPendiente"/> lo mantiene el módulo de Ventas (ventas a crédito y abonos)
/// y no debe recibirse desde la API.
/// </summary>
public class Cliente : BaseEntity
{
    public string Rut { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? Contacto { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string Email { get; set; } = string.Empty;
    public decimal LimiteCredito { get; set; }
    public decimal SaldoPendiente { get; set; }
}
