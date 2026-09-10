using ERP.Application.DTOs;
using ERP.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var response = await _authService.LoginAsync(request);
        if (response == null)
        {
            return Unauthorized(new { mensaje = "Credenciales incorrectas o usuario inactivo." });
        }
        return Ok(response);
    }

    [HttpPost("register")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto request)
    {
        var creador = User.Identity?.Name ?? "Admin";
        var usuario = await _authService.RegisterUserAsync(request, creador);
        return Ok(new { mensaje = "Usuario creado exitosamente", usuario.Id, usuario.Email });
    }
}
