// Módulo: Ventas — Responsable: María
using System.ComponentModel.DataAnnotations;

namespace ERP.Application.DTOs;

/// <summary>Cliente expuesto por la API.</summary>
public record ClienteDto(int Id, string Rut, string RazonSocial, string? Giro, string? Contacto,
    string? Direccion, string? Telefono, string Email, decimal LimiteCredito, decimal SaldoPendiente, bool Activo);

/// <summary>
/// Datos para crear o actualizar un cliente. No incluye SaldoPendiente (lo calcula Ventas),
/// evitando el overposting del endpoint anterior (M8).
/// </summary>
public record GuardarClienteDto(
    [Required, StringLength(20)] string Rut,
    [Required, StringLength(200)] string RazonSocial,
    [StringLength(200)] string? Giro,
    [StringLength(150)] string? Contacto,
    [StringLength(250)] string? Direccion,
    [StringLength(30)] string? Telefono,
    [Required, EmailAddress] string Email,
    [Range(typeof(decimal), "0", "999999999999")] decimal LimiteCredito);
