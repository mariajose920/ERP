// Módulo: Seguridad (transversal T4) — Responsable: Cristóbal
namespace ERP.Application.Common;

/// <summary>
/// Configuración JWT centralizada (sección "Jwt"). La clave NO debe quedar en appsettings.json:
/// se entrega por variable de entorno <c>Jwt__SecretKey</c> (archivo .env) o user-secrets.
/// </summary>
public class JwtSettings
{
    /// <summary>Nombre de la sección de configuración.</summary>
    public const string Seccion = "Jwt";

    /// <summary>Clave HMAC-SHA256 (mínimo 32 caracteres).</summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Emisor del token.</summary>
    public string Issuer { get; set; } = "ERPIntegrado";

    /// <summary>Audiencia del token.</summary>
    public string Audience { get; set; } = "ERPIntegradoClient";

    /// <summary>Vigencia del token en horas.</summary>
    public int ExpiracionHoras { get; set; } = 12;

    /// <summary>Valida la configuración al iniciar la aplicación.</summary>
    public void Validar()
    {
        if (string.IsNullOrWhiteSpace(SecretKey) || SecretKey.Length < 32)
        {
            throw new InvalidOperationException(
                "Falta la clave JWT (Jwt__SecretKey) o tiene menos de 32 caracteres. Configúrela en .env o user-secrets.");
        }
    }
}
