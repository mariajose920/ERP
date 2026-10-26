// Módulo: Seguridad — Responsable: Cristóbal
using ERP.Api.Infraestructura;
using ERP.Application.Common;
using ERP.Application.DTOs;
using ERP.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

/// <summary>Autenticación (JWT) y alta de usuarios.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Inicia sesión y devuelve un JWT con rol y permisos.</summary>
    /// <response code="200">Credenciales válidas; incluye el token.</response>
    /// <response code="401">Credenciales incorrectas o usuario inactivo.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var response = await _authService.LoginAsync(request);
        if (response == null)
        {
            return Unauthorized(new { mensaje = "Credenciales incorrectas o usuario inactivo." });
        }
        return Ok(response);
    }

    /// <summary>Crea un usuario (requiere permiso usuarios.administrar).</summary>
    /// <response code="200">Usuario creado.</response>
    /// <response code="400">Correo duplicado o rol inexistente.</response>
    [HttpPost("register")]
    [Authorize(Policy = PermisosConst.UsuariosAdministrar)]
    [ProducesResponseType(typeof(UsuarioCreadoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto request)
    {
        var usuario = await _authService.RegisterUserAsync(request, User.ObtenerUsuario());
        return Ok(usuario);
    }

    /// <summary>
    /// Credenciales de demostración (Administrador y Cajero) para el recuadro de login.
    /// Solo responde en el entorno Development; en otros entornos devuelve 404.
    /// </summary>
    /// <response code="200">Lista de credenciales de demo.</response>
    /// <response code="404">No es entorno de desarrollo.</response>
    [HttpGet("demo-credenciales")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<CredencialDemoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCredencialesDemo(
        [FromServices] IWebHostEnvironment entorno,
        [FromServices] IConfiguration configuracion,
        [FromServices] ERP.Infrastructure.Data.ErpDbContext context)
    {
        if (!entorno.IsDevelopment()) return NotFound();

        var lista = new List<CredencialDemoDto>();
        var adminPassword = configuracion["Seed:AdminPassword"];
        if (!string.IsNullOrWhiteSpace(adminPassword))
            lista.Add(new CredencialDemoDto("Administrador", ERP.Infrastructure.Data.DbInitializer.EmailAdminPorDefecto, adminPassword, "index_erp.html"));

        // El seed reutiliza CAJA1 como cajero; si no existe, crea cajero@erp.com.
        var existeCaja1 = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync(context.Usuarios, u => u.Email == "CAJA1");
        if (existeCaja1)
            lista.Add(new CredencialDemoDto("Cajero", "CAJA1", ERP.Infrastructure.Data.DbInitializer.PasswordCajerosSeed, "pantalla_caja_inicio.html"));
        else if (!string.IsNullOrWhiteSpace(configuracion["Seed:CajeroPassword"]))
            lista.Add(new CredencialDemoDto("Cajero", ERP.Infrastructure.Data.DbInitializer.EmailCajeroPorDefecto, configuracion["Seed:CajeroPassword"]!, "pantalla_caja_inicio.html"));

        return Ok(lista);
    }
}
