using Cheer.Application.Interfaces;
using Cheer.Application.Services;
using Cheer.Domain.Interfaces;
using Cheer.Infrastructure.Data;
using Cheer.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? Array.Empty<string>();
var corsEnv = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS");
if (allowedOrigins.Length == 0 && !string.IsNullOrWhiteSpace(corsEnv))
{
    allowedOrigins = corsEnv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (allowedOrigins.Length == 0)
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .SetIsOriginAllowedToAllowWildcardSubdomains();
        }
    });
});

// Database
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
                       ?? builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' or env var 'DATABASE_URL' must be configured.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        connectionString,
        o => o.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null)));

// Dependency Injection
builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<IChampionshipRepository, ChampionshipRepository>();
builder.Services.AddScoped<IChampionshipService, ChampionshipService>();

var app = builder.Build();

// Swagger (deixe habilitado enquanto testa)
app.UseSwagger();
app.UseSwaggerUI();

// Não redirecionar HTTPS em produção
if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

// Pasta de uploads (configurável via env var p/ volume persistente; fallback: wwwroot/uploads)
var uploadsFolder = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("UPLOADS_PATH"))
    ? Environment.GetEnvironmentVariable("UPLOADS_PATH")!
    : Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

if (!Directory.Exists(uploadsFolder))
{
    Directory.CreateDirectory(uploadsFolder);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsFolder),
    RequestPath = "/uploads"
});

// CORS
app.UseCors();

app.UseAuthorization();

// Healthcheck
app.MapGet("/", () => Results.Ok(new
{
    status = "online",
    environment = app.Environment.EnvironmentName,
    time = DateTime.UtcNow
}));

// Controllers
app.MapControllers();

app.Run();