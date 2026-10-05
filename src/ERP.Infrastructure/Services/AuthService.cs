using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ERP.Application.DTOs;
using ERP.Application.Interfaces;
using ERP.Domain.Entities.Seguridad;
using ERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ERP.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ErpDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ErpDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        var user = await _context.Usuarios
            .Include(u => u.Rol!)
                .ThenInclude(r => r.RolPermisos)
                    .ThenInclude(rp => rp.Permiso)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower() && u.Activo);

        if (user == null || !DbInitializer.VerifyPassword(request.Password, user.PasswordHash))
        {
            return null;
        }

        user.UltimoAcceso = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var rolNombre = user.Rol?.Nombre ?? "SinRol";
        var permisos = user.Rol?.RolPermisos.Select(rp => rp.Permiso.Codigo).ToList() ?? new List<string>();

        var token = GenerateJwtToken(user, rolNombre, permisos);

        return new LoginResponseDto(token, user.NombreCompleto, user.Email, rolNombre, permisos);
    }

    public async Task<Usuario> RegisterUserAsync(RegisterUserDto request, string creadorEmail)
    {
        if (await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower()))
        {
            throw new InvalidOperationException("El correo ya se encuentra registrado.");
        }

        var usuario = new Usuario
        {
            NombreCompleto = request.NombreCompleto,
            Email = request.Email,
            Rut = request.Rut,
            PasswordHash = DbInitializer.HashPassword(request.Password),
            RolId = request.RolId,
            CreadoPor = creadorEmail,
            Activo = true
        };

        await _context.Usuarios.AddAsync(usuario);
        await _context.SaveChangesAsync();

        return usuario;
    }

    private string GenerateJwtToken(Usuario user, string rol, List<string> permisos)
    {
        var secretKey = _configuration["Jwt:SecretKey"] ?? "ERP_Super_Secret_Key_For_Development_Environment_2026";
        var issuer = _configuration["Jwt:Issuer"] ?? "ERPIntegrado";
        var audience = _configuration["Jwt:Audience"] ?? "ERPIntegradoClient";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.NombreCompleto),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, rol)
        };

        foreach (var permiso in permisos)
        {
            claims.Add(new Claim("permiso", permiso));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(12),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
