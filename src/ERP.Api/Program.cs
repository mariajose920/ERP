using System.Text;
using System.Text.Json.Serialization;
using ERP.Application.Interfaces;
using ERP.Infrastructure.Data;
using ERP.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Load .env file into environment variables if present
var rootEnvPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env");
if (File.Exists(rootEnvPath))
{
    foreach (var line in File.ReadAllLines(rootEnvPath))
    {
        var trimmed = line.Trim();
        if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#")) continue;
        var parts = trimmed.Split('=', 2);
        if (parts.Length == 2)
        {
            var key = parts[0].Trim();
            var val = parts[1].Trim().Trim('"');
            Environment.SetEnvironmentVariable(key, val);
        }
    }
}

// 1. Connection String to Supabase PostgreSQL
var defaultConnection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=aws-0-us-east-2.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.frwijnhngknsktcrxdfp;Password=ProyectoERP;SSL Mode=Require;Trust Server Certificate=true;";

builder.Services.AddDbContext<ErpDbContext>(options =>
{
    options.UseNpgsql(defaultConnection, npgsqlOptions =>
    {
        npgsqlOptions.MigrationsAssembly("ERP.Infrastructure");
    });
});

// 2. Dependency Injection for Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IComprasService, ComprasService>();
builder.Services.AddScoped<IVentasService, VentasService>();
builder.Services.AddScoped<IInventarioService, InventarioService>();
builder.Services.AddScoped<IContabilidadService, ContabilidadService>();

// 3. JSON Configuration to handle circular references
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// 4. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 5. JWT Authentication
var secretKey = builder.Configuration["Jwt:SecretKey"] ?? "ERP_Super_Secret_Key_For_Development_Environment_2026_Secure_Key";
var issuer = builder.Configuration["Jwt:Issuer"] ?? "ERPIntegrado";
var audience = builder.Configuration["Jwt:Audience"] ?? "ERPIntegradoClient";

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
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

// 6. Swagger / OpenAPI Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Auto-migrate & Seed Database on Startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ErpDbContext>();
        context.Database.EnsureCreated();
        await DbInitializer.SeedAsync(context);
        Console.WriteLine("===============================================================");
        Console.WriteLine("[SUPABASE POSTGRESQL] Conexion exitosa y tablas inicializadas!");
        Console.WriteLine("===============================================================");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"--> Error conectando con la Base de Datos: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"--> Inner Exception: {ex.InnerException.Message}");
        }
    }
}

// HTTP pipeline
if (app.Environment.IsDevelopment() || true)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ERP Integrado API v1");
        c.RoutePrefix = string.Empty; // Swagger at root URL
    });
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
