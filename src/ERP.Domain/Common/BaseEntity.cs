namespace ERP.Domain.Common;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaModificacion { get; set; }
    public string? CreadoPor { get; set; }
    public string? ModificadoPor { get; set; }
    public bool Activo { get; set; } = true;
}
