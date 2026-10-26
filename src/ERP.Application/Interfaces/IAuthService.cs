// Módulo: Seguridad — Responsable: Cristóbal
using ERP.Application.DTOs;

namespace ERP.Application.Interfaces;

/// <summary>Autenticación y alta de usuarios.</summary>
public interface IAuthService
{
    /// <summary>Valida credenciales y emite un JWT con rol y permisos. Devuelve null si son inválidas.</summary>
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);

    /// <summary>Crea un usuario. Lanza InvalidOperationException si el correo ya existe (→ 400).</summary>
    Task<UsuarioCreadoDto> RegisterUserAsync(RegisterUserDto request, string creadorEmail);
}
