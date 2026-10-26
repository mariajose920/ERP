// Módulo: Seguridad (K7) — Responsable: Cristóbal
using System.Security.Cryptography;
using System.Text;

namespace ERP.Infrastructure.Services.Seguridad;

/// <summary>
/// Hash de contraseñas con PBKDF2-HMAC-SHA256 (100.000 iteraciones, sal aleatoria de 16 bytes).
/// Formato almacenado: <c>PBKDF2$iteraciones$salBase64$hashBase64</c>.
/// Mantiene compatibilidad con los hashes SHA-256 sin sal del seed anterior: se verifican y
/// se marcan para re-hash, que AuthService aplica en el siguiente login exitoso.
/// </summary>
public static class HashContrasenas
{
    private const string Prefijo = "PBKDF2";
    private const int Iteraciones = 100_000;
    private const int LargoSal = 16;
    private const int LargoHash = 32;

    /// <summary>Genera el hash PBKDF2 de una contraseña.</summary>
    public static string Hashear(string contrasena)
    {
        var sal = RandomNumberGenerator.GetBytes(LargoSal);
        var hash = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, Iteraciones, HashAlgorithmName.SHA256, LargoHash);
        return $"{Prefijo}${Iteraciones}${Convert.ToBase64String(sal)}${Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Verifica una contraseña. <paramref name="requiereRehash"/> es true si el hash almacenado
    /// usa el formato antiguo (SHA-256 sin sal) y debe reemplazarse.
    /// </summary>
    public static bool Verificar(string contrasena, string hashAlmacenado, out bool requiereRehash)
    {
        requiereRehash = false;
        if (string.IsNullOrEmpty(hashAlmacenado)) return false;

        var partes = hashAlmacenado.Split('$');
        if (partes.Length == 4 && partes[0] == Prefijo && int.TryParse(partes[1], out var iter))
        {
            var sal = Convert.FromBase64String(partes[2]);
            var esperado = Convert.FromBase64String(partes[3]);
            var calculado = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, iter, HashAlgorithmName.SHA256, esperado.Length);
            return CryptographicOperations.FixedTimeEquals(calculado, esperado);
        }

        // Formato heredado: SHA-256 en Base64 sin sal.
        var legado = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(contrasena)));
        var ok = CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(legado), Encoding.UTF8.GetBytes(hashAlmacenado));
        requiereRehash = ok;
        return ok;
    }
}
