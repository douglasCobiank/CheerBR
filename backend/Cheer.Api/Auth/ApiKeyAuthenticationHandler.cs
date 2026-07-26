using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Cheer.Api.Auth;

/// <summary>
/// AuthenticationHandler basado em header <c>X-API-Key</c>.
///
/// Por que AuthenticationHandler e nao apenas AuthorizationHandler? Porque
/// para disparar 401 (Challenge) o pipeline precisa de um scheme de
/// autenticacao registrado (AuthenticationService.ChallengeAsync exige um
/// DefaultChallengeScheme). Sem isso o [Authorize] crashava com
/// InvalidOperationException.
///
/// Fluxo:
///   1. [Authorize(Policy="ApiKey")] no endpoint dispara AuthenticateAsync
///      para identificar o usuario (ouve o AuthenticationHandler).
///   2. Se AuthenticateAsync retornar Failure -> ChallengeAsync -> 401.
///   3. Se AuthenticateAsync retornar Success mas a policy falhar (ex: role) -> 403.
///
/// Aqui AuthenticateAsync ja valida a chave contra ADMIN_API_KEY. Se OK,
/// retorna um ClaimsPrincipal generico ("admin"). Se ausente/invalida,
/// retorna NoResult() e o pipeline envia 401 quando o scheme e desafiado.
/// </summary>
public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    public const string SchemeName = "ApiKey";

    private readonly ILogger<ApiKeyAuthenticationHandler> _logger;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
        _logger = logger.CreateLogger<ApiKeyAuthenticationHandler>();
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var expected = Options.ExpectedApiKey;
        if (string.IsNullOrEmpty(expected))
        {
            _logger.LogError("ADMIN_API_KEY nao configurada. Requests autenticados serao recusados.");
            return Task.FromResult(AuthenticateResult.Fail("Servidor sem ADMIN_API_KEY configurada."));
        }

        if (!Request.Headers.TryGetValue("X-API-Key", out var provided) || string.IsNullOrWhiteSpace(provided.ToString()))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (!CryptographicEquals(provided.ToString(), expected))
        {
            _logger.LogWarning("X-API-Key invalida (IP {Ip}).", Request.HttpContext.Connection.RemoteIpAddress);
            return Task.FromResult(AuthenticateResult.Fail("X-API-Key invalida."));
        }

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "admin"),
            new Claim(ClaimTypes.Role, "admin"),
        }, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        // Resposta 401 canonica quando a chave nao foi fornecida ou falhou.
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.ContentType = "application/problem+json";
        return Response.WriteAsJsonAsync(new
        {
            type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.2",
            title = "Unauthorized",
            status = 401,
            detail = "Header X-API-Key ausente ou invalida.",
            traceId = Request.HttpContext.TraceIdentifier,
        });
    }

    protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        Response.ContentType = "application/problem+json";
        return Response.WriteAsJsonAsync(new
        {
            type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.3",
            title = "Forbidden",
            status = 403,
            traceId = Request.HttpContext.TraceIdentifier,
        });
    }

    private static bool CryptographicEquals(string a, string b)
    {
        if (a.Length != b.Length) return false;
        var diff = 0;
        for (var i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
        return diff == 0;
    }
}

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public string ExpectedApiKey { get; set; } = string.Empty;
}
