// Módulo: Compras — Responsable: Adán
using System.ComponentModel.DataAnnotations;

namespace ERP.Application.DTOs;

/// <summary>Proveedor expuesto por la API.</summary>
public record ProveedorDto(int Id, string Rut, string RazonSocial, string? Giro, string? Contacto,
    string? Direccion, string? Telefono, string Email, string CondicionesPago, bool Activo);

/// <summary>Datos para crear o actualizar un proveedor.</summary>
public record GuardarProveedorDto(
    [Required, StringLength(20)] string Rut,
    [Required, StringLength(200)] string RazonSocial,
    [StringLength(200)] string? Giro,
    [StringLength(150)] string? Contacto,
    [StringLength(250)] string? Direccion,
    [StringLength(30)] string? Telefono,
    [Required, EmailAddress] string Email,
    [StringLength(50)] string? CondicionesPago);
