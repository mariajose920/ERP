// Módulo: Integración (puente localStorage → SQL, K6, OBSOLETO) — Responsable: Cristóbal
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

/// <summary>
/// Persistencia en SQL Server de los datos que las pantallas guardaban en localStorage (claves erp_*).
/// OBSOLETO (ronda 2): ninguna pantalla lo usa; todas consumen los endpoints reales y erp-sync.js
/// se retiró. Se conserva solo para leer datos históricos; la tabla dbo.ErpKeyValue NO se borra.
/// La tabla dbo.ErpKeyValue se crea con SQL en Program.cs (no forma parte del modelo EF).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = ERP.Application.Common.PermisosConst.UsuariosAdministrar)]
[Obsolete("Puente localStorage→SQL retirado: use los endpoints de cada módulo.")]
public class StoreController : ControllerBase
{
    private const string PrefijoClave = "erp_";
    private readonly ErpDbContext _context;

    public StoreController(ErpDbContext context)
    {
        _context = context;
    }

    /// <summary>Par clave/valor almacenado.</summary>
    public class StoreItem
    {
        /// <summary>Clave (debe comenzar con "erp_").</summary>
        public string Clave { get; set; } = "";
        /// <summary>Valor serializado (JSON de la pantalla).</summary>
        public string Valor { get; set; } = "";
    }

    /// <summary>Devuelve todas las claves erp_* como diccionario.</summary>
    /// <response code="200">Diccionario clave → valor.</response>
    [HttpGet]
    [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var items = await _context.Database
            .SqlQueryRaw<StoreItem>("SELECT Clave, Valor FROM ErpKeyValue")
            .ToListAsync();
        return Ok(items.ToDictionary(i => i.Clave, i => i.Valor));
    }

    /// <summary>Crea o actualiza una clave erp_*.</summary>
    /// <response code="204">Guardado.</response>
    /// <response code="400">La clave no comienza con "erp_".</response>
    [HttpPut("{clave}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Put(string clave, [FromBody] StoreItem body)
    {
        if (!clave.StartsWith(PrefijoClave)) return BadRequest(new { error = "Clave inválida: debe comenzar con 'erp_'." });
        await _context.Database.ExecuteSqlRawAsync(
            @"MERGE ErpKeyValue AS t
              USING (SELECT {0} AS Clave, {1} AS Valor) AS s ON t.Clave = s.Clave
              WHEN MATCHED THEN UPDATE SET Valor = s.Valor, Actualizado = SYSUTCDATETIME()
              WHEN NOT MATCHED THEN INSERT (Clave, Valor) VALUES (s.Clave, s.Valor);",
            clave, body.Valor ?? "");
        return NoContent();
    }

    /// <summary>Elimina una clave erp_*.</summary>
    /// <response code="204">Eliminada.</response>
    /// <response code="400">La clave no comienza con "erp_".</response>
    [HttpDelete("{clave}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(string clave)
    {
        if (!clave.StartsWith(PrefijoClave)) return BadRequest(new { error = "Clave inválida: debe comenzar con 'erp_'." });
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM ErpKeyValue WHERE Clave = {0}", clave);
        return NoContent();
    }
}
