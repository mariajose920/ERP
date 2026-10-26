// Módulo: Transversal — Responsable: Cristóbal
namespace ERP.Domain.Common;

/// <summary>Campos comunes de auditoría y borrado lógico para todas las entidades.</summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaModificacion { get; set; }
    public string? CreadoPor { get; set; }
    public string? ModificadoPor { get; set; }
    public bool Activo { get; set; } = true;
}
