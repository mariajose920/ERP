// Módulo: Transversal (T5) — Responsable: Cristóbal
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Infraestructura;

/// <summary>
/// Manejo global de errores (T5). Reemplaza los try/catch repetidos en los controladores y
/// responde siempre con <see cref="ProblemDetails"/>:
/// <list type="bullet">
/// <item>KeyNotFoundException → 404</item>
/// <item>InvalidOperationException / ArgumentException (reglas de negocio) → 400</item>
/// <item>UnauthorizedAccessException → 403</item>
/// <item>DbUpdateException (p. ej. RUT/SKU duplicado) → 409</item>
/// <item>Cualquier otra → 500 (sin exponer el detalle interno)</item>
/// </list>
/// Para compatibilidad con las pantallas existentes, el mensaje también se entrega en la propiedad "error".
/// </summary>
public class ManejadorExcepcionesGlobal : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetails;
    private readonly ILogger<ManejadorExcepcionesGlobal> _logger;

    public ManejadorExcepcionesGlobal(IProblemDetailsService problemDetails, ILogger<ManejadorExcepcionesGlobal> logger)
    {
        _problemDetails = problemDetails;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, titulo) = exception switch
        {
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            InvalidOperationException or ArgumentException => (StatusCodes.Status400BadRequest, "Operación no válida"),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Acceso denegado"),
            DbUpdateException => (StatusCodes.Status409Conflict, "Conflicto al guardar los datos"),
            _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor")
        };

        if (status >= 500)
            _logger.LogError(exception, "Error no controlado en {Ruta}", httpContext.Request.Path);
        else
            _logger.LogInformation("Solicitud rechazada ({Status}) en {Ruta}: {Mensaje}", status, httpContext.Request.Path, exception.Message);

        var detalle = status switch
        {
            StatusCodes.Status500InternalServerError => "Ocurrió un error inesperado. Revise el log del servidor.",
            StatusCodes.Status409Conflict => "El registro entra en conflicto con datos existentes (por ejemplo, un código o RUT duplicado).",
            _ => exception.Message
        };

        httpContext.Response.StatusCode = status;
        var problema = new ProblemDetails { Status = status, Title = titulo, Detail = detalle, Instance = httpContext.Request.Path };
        problema.Extensions["error"] = detalle;

        return await _problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problema,
            Exception = exception
        });
    }
}
