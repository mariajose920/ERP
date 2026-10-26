// Módulo: Transversal (arranque de la API) — Responsable: Cristóbal
using System.Text;
using System.Text.Json.Serialization;
using ERP.Api.Infraestructura;
using ERP.Application.Common;
using ERP.Application.Interfaces;
using ERP.Infrastructure.Data;
using ERP.Infrastructure.Services;
using ERP.Infrastructure.Services.Comun;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// 0. Variables del archivo .env (raíz del repositorio). Se buscan en la carpeta actual y dos niveles
//    arriba (dotnet run desde ERP/ o desde src/ERP.Api). Luego se vuelven a leer las variables de
//    entorno para que la configuración (Jwt__SecretKey, ConnectionStrings__DefaultConnection) las vea.
foreach (var ruta in new[]
{
    Path.Combine(Directory.GetCurrentDirectory(), ".env"),
    Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env")
})
{
    if (!File.Exists(ruta)) continue;
    foreach (var linea in File.ReadAllLines(ruta))
    {
        var limpia = linea.Trim();
        if (string.IsNullOrWhiteSpace(limpia) || limpia.StartsWith("#")) continue;
        var partes = limpia.Split('=', 2);
        if (partes.Length == 2)
            Environment.SetEnvironmentVariable(partes[0].Trim(), partes[1].Trim().Trim('"'));
    }
    break;
}
builder.Configuration.AddEnvironmentVariables();

// 1. Cadena de conexión a SQL Server (appsettings o variable ConnectionStrings__DefaultConnection).
var cadenaConexion = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\mssqllocaldb;Database=ERP_Integrado_Express;Trusted_Connection=True;TrustServerCertificate=True;";

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUsuarioActual, UsuarioActualHttp>();
builder.Services.AddScoped<AuditoriaInterceptor>();

builder.Services.AddDbContext<ErpDbContext>((sp, options) =>
{
    options.UseSqlServer(cadenaConexion, sql => sql.MigrationsAssembly("ERP.Infrastructure"));
    options.AddInterceptors(sp.GetRequiredService<AuditoriaInterceptor>()); // K4
});

// 2. Servicios de negocio (inyección de dependencias).
builder.Services.AddScoped<INumeradorDocumentos, NumeradorDocumentos>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IContabilidadService, ContabilidadService>();
builder.Services.AddScoped<IInventarioService, InventarioService>();
builder.Services.AddScoped<IComprasService, ComprasService>();
builder.Services.AddScoped<IVentasService, VentasService>();

// 3. Controladores y JSON (se ignoran ciclos de navegación de las entidades).
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    // Los enums viajan como texto ("RecibidaParcial", "Anulada"...); también se aceptan números al recibir.
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// 4. Errores globales con ProblemDetails (T5).
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorExcepcionesGlobal>();

// 5. CORS: orígenes en "Cors:OrigenesPermitidos"; si no hay, se permite cualquiera (desarrollo).
var origenes = builder.Configuration.GetSection("Cors:OrigenesPermitidos").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        if (origenes.Length > 0) policy.WithOrigins(origenes);
        else policy.AllowAnyOrigin();
        policy.AllowAnyMethod().AllowAnyHeader();
    });
});

// 6. JWT centralizado en JwtSettings (T4). La clave viene de .env / user-secrets.
var jwt = builder.Configuration.GetSection(JwtSettings.Seccion).Get<JwtSettings>() ?? new JwtSettings();
jwt.Validar();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.Seccion));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt.Issuer,
        ValidAudience = jwt.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey))
    };
});

// 7. Autorización: todo endpoint exige usuario autenticado (T1) y cada permiso es una política (K5).
//    El rol Administrador cumple todas las políticas.
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    foreach (var permiso in PermisosConst.Todos)
    {
        var codigo = permiso.Codigo;
        options.AddPolicy(codigo, p => p.RequireAuthenticatedUser().RequireAssertion(ctx =>
            ctx.User.IsInRole("Administrador") || ctx.User.HasClaim(AuthService.ClaimPermiso, codigo)));
    }
});

// 8. Swagger / OpenAPI con comentarios XML y botón "Authorize" para el JWT.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    foreach (var xml in Directory.GetFiles(AppContext.BaseDirectory, "ERP.*.xml"))
        c.IncludeXmlComments(xml);

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Pegue el token obtenido en POST /api/auth/login"
    });
    c.AddSecurityRequirement(documento => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", documento)] = new List<string>()
    });
});

var app = builder.Build();

// Inicialización de la base de datos y datos semilla.
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
        var creador = context.GetService<IRelationalDatabaseCreator>();
        var tieneHistorial = (await context.Database.GetAppliedMigrationsAsync()).Any();
        var tieneTablas = await creador.ExistsAsync() && await creador.HasTablesAsync();

        if (tieneHistorial || !tieneTablas)
        {
            // T4: el esquema se gestiona con migraciones (antes: EnsureCreated).
            await context.Database.MigrateAsync();
        }
        else
        {
            // BD creada antes con EnsureCreated: no tiene __EFMigrationsHistory y Migrate() fallaría
            // al intentar recrear las tablas. Ver README, sección "Migraciones" (registrar baseline).
            logger.LogWarning("La base de datos no tiene historial de migraciones (fue creada con EnsureCreated). " +
                              "No se aplican migraciones automáticamente; ver README > Migraciones.");
        }

        // Tabla clave-valor del antiguo puente localStorage → SQL (StoreController, OBSOLETO desde la ronda 2).
        // Se conserva la tabla para no perder datos históricos.
        await context.Database.ExecuteSqlRawAsync(@"IF OBJECT_ID('dbo.ErpKeyValue','U') IS NULL
            CREATE TABLE dbo.ErpKeyValue (
                Clave NVARCHAR(200) NOT NULL PRIMARY KEY,
                Valor NVARCHAR(MAX) NOT NULL,
                Actualizado DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME());");

        await DbInitializer.SeedAsync(context, builder.Configuration["Seed:AdminPassword"], builder.Configuration["Seed:CajeroPassword"]);
        logger.LogInformation("[SQL SERVER] Conexión exitosa y datos iniciales verificados.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error inicializando la base de datos: {Mensaje}", ex.Message);
    }
}

// Canal HTTP.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    // T4: Swagger solo en desarrollo.
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ERP Integrado API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("Frontend");

// Frontend estático (fuente única: wwwroot, T8). Los archivos estáticos no requieren JWT;
// las pantallas redirigen a login.html si no hay sesión y la API exige el token.
var defaultFilesOptions = new DefaultFilesOptions();
defaultFilesOptions.DefaultFileNames.Clear();
defaultFilesOptions.DefaultFileNames.Add("index_erp.html");
app.UseDefaultFiles(defaultFilesOptions);
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
