// Módulo: Seguridad (auditoría, K4) — Responsable: Cristóbal
using System.Security.Claims;
using ERP.Application.Interfaces;

namespace ERP.Api.Infraestructura;

/// <summary>Obtiene el usuario autenticado desde el HttpContext (claims del JWT).</summary>
public class UsuarioActualHttp : IUsuarioActual
{
    private readonly IHttpContextAccessor _accessor;

    public UsuarioActualHttp(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    /// <inheritdoc />
    public string Email => _accessor.HttpContext?.User.ObtenerUsuario() ?? "Sistema";

    /// <inheritdoc />
    public string? Ip => _accessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}

/// <summary>Extensiones para leer el usuario desde los claims.</summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>Email del usuario autenticado (o su nombre si no hay email); "Sistema" si es anónimo.</summary>
    public static string ObtenerUsuario(this ClaimsPrincipal? usuario)
    {
        if (usuario?.Identity?.IsAuthenticated != true) return "Sistema";
        return usuario.FindFirstValue(ClaimTypes.Email)
            ?? usuario.Identity.Name
            ?? "Sistema";
    }

    /// <summary>True si el usuario es Administrador o tiene el claim de permiso indicado.</summary>
    public static bool TienePermiso(this ClaimsPrincipal usuario, string codigo) =>
        usuario.IsInRole(ERP.Application.Common.PermisosConst.RolAdministrador) ||
        usuario.HasClaim(ERP.Infrastructure.Services.AuthService.ClaimPermiso, codigo);
}
