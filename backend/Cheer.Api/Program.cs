using Cheer.Api.ExceptionHandling;
using Cheer.Application.Interfaces;
using Cheer.Application.Services;
using Cheer.Domain.Interfaces;
using Cheer.Infrastructure.Data;
using Cheer.Infrastructure.Repositories;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ProblemDetails: contratos de erro padronizados (RFC 7807)
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ctx =>
    {
        ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
    };
});

// Handler custom: mapeia excecoes de dominio (ex: NotFoundException) para
// codigos HTTP corretos (404) em vez de 500 generico.
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();

// Configure CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? Array.Empty<string>();
var corsEnv = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS");
if (allowedOrigins.Length == 0 && !string.IsNullOrWhiteSpace(corsEnv))
{
    allowedOrigins = corsEnv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}

if (builder.Environment.IsProduction() && allowedOrigins.Length == 0)
{
    throw new InvalidOperationException(
        "CORS_ALLOWED_ORIGINS (ou Cors:AllowedOrigins) deve estar definido e nao vazio em Production. " +
        "Recusar AllowAnyOrigin combinado com API sem auth e um risco de seguranca.");
}

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (allowedOrigins.Length == 0)
        {
            // Apenas em ambientes nao-Production
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

// Swagger apenas em Development (evita enumeracao publica da API em producao)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Exception handler global: converte excecoes nao tratadas em ProblemDetails
// (substitui os padroes catch { NotFound } que escondiam erros reais como 404)
app.UseExceptionHandler();

// Nao redirecionar HTTPS em producao (TLS presumido no proxy/ingress)
if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

// Pasta de uploads (configuravel via env var p/ volume persistente; fallback: wwwroot/uploads)
var uploadsFolder = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("UPLOADS_PATH"))
    ? Environment.GetEnvironmentVariable("UPLOADS_PATH")!
    : Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

if (!Directory.Exists(uploadsFolder))
{
    Directory.CreateDirectory(uploadsFolder);
}

// Static files com cabecalhos de seguranca (mitiga MIME-sniffing em uploads)
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsFolder),
    RequestPath = "/uploads",
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        ctx.Context.Response.Headers["Cache-Control"] = "no-store";
    }
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
