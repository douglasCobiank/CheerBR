using Cheer.Api.Auth;
using Cheer.Api.ExceptionHandling;
using Cheer.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Cheer.Application.Services;
using Cheer.Domain.Interfaces;
using Cheer.Infrastructure.Data;
using Cheer.Infrastructure.Repositories;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.RateLimiting;

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

// Autenticacao via header X-API-Key e AuthorizationHandler associado.
// Veja Auth/ApiKeyAuthenticationHandler.cs para os detalhes.
builder.Services
    .AddAuthentication(ApiKeyAuthenticationHandler.SchemeName)
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationHandler.SchemeName,
        options =>
        {
            options.ExpectedApiKey =
                builder.Configuration["ADMIN_API_KEY"]
                ?? Environment.GetEnvironmentVariable("ADMIN_API_KEY")
                ?? string.Empty;
        });

builder.Services.AddAuthorizationBuilder()
    .AddApiKeyAuthorization();

// Rate limiting: janela fixa por IP para proteger o endpoint de upload de logos
// (5 MB por request, sem auth forte na camada de CDN). 5 requests permitidos
// a cada 30 segundos por IP; excedente retorna 429.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("logo", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromSeconds(30);
        opt.QueueLimit = 0;
    });
});

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

// Storage: R2 (se configurado) ou disco local. Singleton porque mantem
// o cliente S3 e as configuracoes de bucket por toda a vida da app.
builder.Services.AddSingleton<IStorageService, R2StorageService>();

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

// Rate limiting (antes de UseAuthorization para rejeitar cedo)
app.UseRateLimiter();

// Pipeline de autorizacao: Authentication identifica o caller via X-API-Key.
// Authorization valida a policy "ApiKey" em todos os endpoints [Authorize].
app.UseAuthentication();
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
