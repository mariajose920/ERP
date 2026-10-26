// Módulo: Seguridad (administración de usuarios y roles) — Responsable: Cristóbal
using System.ComponentModel.DataAnnotations;

namespace ERP.Application.DTOs;

/// <summary>Usuario para la pantalla de administración (sin hash de contraseña).</summary>
public record UsuarioDto(int Id, string NombreCompleto, string Email, string Rut, int RolId, string Rol, bool Activo, DateTime? UltimoAcceso, decimal? PorcentajeComision);

/// <summary>Cambios sobre un usuario. Si <c>NuevaPassword</c> viene informada, se reemplaza.</summary>
public record ActualizarUsuarioDto(
    [Required, StringLength(150)] string NombreCompleto,
    [Required, StringLength(20)] string Rut,
    [Range(1, int.MaxValue)] int RolId,
    bool Activo,
    [MinLength(8)] string? NuevaPassword,
    [Range(typeof(decimal), "0", "100", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true, ErrorMessage = "La comisión debe estar entre 0 y 100.")] decimal? PorcentajeComision = null);

/// <summary>Rol con sus códigos de permiso.</summary>
public record RolDto(int Id, string Nombre, string Descripcion, IReadOnlyList<string> Permisos, int CantidadUsuarios);

/// <summary>Permiso disponible.</summary>
public record PermisoDto(string Codigo, string Modulo, string Descripcion);

/// <summary>Reemplaza el conjunto de permisos de un rol.</summary>
public record ActualizarPermisosRolDto([Required] List<string> Permisos);

/// <summary>Credencial de demostración (solo se expone en Development).</summary>
public record CredencialDemoDto(string Perfil, string Usuario, string Password, string Inicio);
