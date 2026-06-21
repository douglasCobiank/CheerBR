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
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
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

// Pasta de uploads
var uploadsFolder = Path.Combine(
    Directory.GetCurrentDirectory(),
    "wwwroot",
    "uploads");

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