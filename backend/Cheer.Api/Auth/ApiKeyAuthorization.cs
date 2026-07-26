using Microsoft.AspNetCore.Authorization;

namespace Cheer.Api.Auth;

/// <summary>
/// Requisito de autorizacao: qualquer request com authentication ticket
/// valido vindo do scheme "ApiKey" e aprovado. Toda a logica de validar
/// a header X-API-Key fica em ApiKeyAuthenticationHandler.
/// </summary>
public class ApiKeyRequirement : IAuthorizationRequirement
{
}

public class ApiKeyRequirementHandler : AuthorizationHandler<ApiKeyRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ApiKeyRequirement requirement)
    {
        // Se a autenticacao via AuthenticationHandler teve sucesso, o user
        // esta autenticado. A policy aprova automaticamente.
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            context.Succeed(requirement);
        }
        // Caso contrario nao chamamos Fail() — o ChallengeAsync do scheme
        // cuida de escrever 401.
        return Task.CompletedTask;
    }
}

public static class ApiKeyAuthorizationExtensions
{
    public const string PolicyName = "ApiKey";

    public static AuthorizationBuilder AddApiKeyAuthorization(this AuthorizationBuilder builder)
    {
        builder.Services.AddSingleton<IAuthorizationHandler, ApiKeyRequirementHandler>();
        return builder.AddPolicy(PolicyName, policy =>
        {
            policy.AuthenticationSchemes.Add(ApiKeyAuthenticationHandler.SchemeName);
            policy.Requirements.Add(new ApiKeyRequirement());
        });
    }
}
