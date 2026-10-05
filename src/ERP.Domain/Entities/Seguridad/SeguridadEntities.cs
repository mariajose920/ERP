using ERP.Domain.Common;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities.Seguridad;

public class Usuario : BaseEntity
{
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Rut { get; set; } = string.Empty;
    public int RolId { get; set; }
    public Rol? Rol { get; set; }
    public DateTime? UltimoAcceso { get; set; }
}

public class Rol : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
}

public class Permiso : BaseEntity
{
    public string Codigo { get; set; } = string.Empty; // e.g. "compras.proveedores.leer"
    public string Modulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
}

public class RolPermiso
{
    public int RolId { get; set; }
    public Rol Rol { get; set; } = null!;
    public int PermisoId { get; set; }
    public Permiso Permiso { get; set; } = null!;
}

public class Auditoria : BaseEntity
{
    public string Modulo { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty; // Creación, Modificación, Anulación, Ajuste
    public string Entidad { get; set; } = string.Empty;
    public string? EntidadId { get; set; }
    public string UsuarioEmail { get; set; } = string.Empty;
    public string? Detalles { get; set; }
    public string? IpAddress { get; set; }
}
