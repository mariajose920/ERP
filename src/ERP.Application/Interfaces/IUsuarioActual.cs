// Módulo: Seguridad (auditoría, K4) — Responsable: Cristóbal
namespace ERP.Application.Interfaces;

/// <summary>Datos del usuario autenticado en la petición actual (se usa para auditoría).</summary>
public interface IUsuarioActual
{
    /// <summary>Email del usuario autenticado o "Sistema" si no hay petición HTTP/usuario.</summary>
    string Email { get; }

    /// <summary>IP de origen de la petición, si está disponible.</summary>
    string? Ip { get; }
}
