// Módulo: Seguridad — Responsable: Cristóbal
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ERP.Application.Common;
using ERP.Application.DTOs;
using ERP.Application.Interfaces;
using ERP.Domain.Entities.Seguridad;
using ERP.Infrastructure.Data;
using ERP.Infrastructure.Services.Seguridad;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ERP.Infrastructure.Services;

/// <summary>Login con JWT (rol + permisos como claims) y alta de usuarios.</summary>
public class AuthService : IAuthService
{
    /// <summary>Tipo de claim con el que se emiten los permisos (lo usan las políticas RBAC).</summary>
    public const string ClaimPermiso = "permiso";

    private readonly ErpDbContext _context;
    private readonly JwtSettings _jwt;

    public AuthService(ErpDbContext context, IOptions<JwtSettings> jwt)
    {
        _context = context;
        _jwt = jwt.Value;
    }

    /// <inheritdoc />
    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        var email = request.Email.Trim().ToLower();
        var user = await _context.Usuarios
            .Include(u => u.Rol!)
                .ThenInclude(r => r.RolPermisos)
                    .ThenInclude(rp => rp.Permiso)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email && u.Activo);

        if (user == null || !HashContrasenas.Verificar(request.Password, user.PasswordHash, out var requiereRehash))
        {
            return null;
        }

        // K7: los hashes SHA-256 antiguos se reemplazan por PBKDF2 en el primer login exitoso.
        if (requiereRehash)
        {
            user.PasswordHash = HashContrasenas.Hashear(request.Password);
        }

        user.UltimoAcceso = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var rolNombre = user.Rol?.Nombre ?? "SinRol";
        var permisos = user.Rol?.RolPermisos.Select(rp => rp.Permiso.Codigo).ToList() ?? new List<string>();

        var token = GenerarToken(user, rolNombre, permisos);

        return new LoginResponseDto(token, user.NombreCompleto, user.Email, rolNombre, permisos);
    }

    /// <inheritdoc />
    public async Task<UsuarioCreadoDto> RegisterUserAsync(RegisterUserDto request, string creadorEmail)
    {
        var email = request.Email.Trim();
        if (await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == email.ToLower()))
        {
            throw new InvalidOperationException("El correo ya se encuentra registrado.");
        }

        if (!await _context.Roles.AnyAsync(r => r.Id == request.RolId))
        {
            throw new InvalidOperationException($"El rol {request.RolId} no existe.");
        }

        var usuario = new Usuario
        {
            NombreCompleto = request.NombreCompleto,
            Email = email,
            Rut = request.Rut,
            PasswordHash = HashContrasenas.Hashear(request.Password),
            RolId = request.RolId,
            CreadoPor = creadorEmail,
            Activo = true
        };

        await _context.Usuarios.AddAsync(usuario);
        await _context.SaveChangesAsync();

        return new UsuarioCreadoDto(usuario.Id, usuario.Email, usuario.NombreCompleto, usuario.RolId);
    }

    private string GenerarToken(Usuario user, string rol, List<string> permisos)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.NombreCompleto),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, rol)
        };
        claims.AddRange(permisos.Select(p => new Claim(ClaimPermiso, p)));

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_jwt.ExpiracionHoras),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
