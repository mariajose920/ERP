// Módulo: Seguridad — Responsable: Cristóbal
using System.ComponentModel.DataAnnotations;

namespace ERP.Application.DTOs;

/// <summary>Credenciales para POST /api/auth/login.</summary>
/// <param name="Email">Correo o nombre de usuario registrado.</param>
/// <param name="Password">Contraseña en texto plano (debe viajar solo por HTTPS).</param>
public record LoginRequestDto(
    [Required] string Email,
    [Required] string Password);

/// <summary>Respuesta de login: JWT y datos básicos del usuario.</summary>
public record LoginResponseDto(string Token, string NombreCompleto, string Email, string Rol, List<string> Permisos);

/// <summary>Datos para crear un usuario (solo Administrador).</summary>
public record RegisterUserDto(
    [Required, StringLength(150)] string NombreCompleto,
    [Required, StringLength(150)] string Email,
    [Required, MinLength(8)] string Password,
    [Required, StringLength(20)] string Rut,
    [Range(1, int.MaxValue)] int RolId);

/// <summary>Usuario creado (sin datos sensibles).</summary>
public record UsuarioCreadoDto(int Id, string Email, string NombreCompleto, int RolId);
