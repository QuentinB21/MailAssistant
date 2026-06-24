using System.Security.Claims;
using MailAssistant.Application.Abstractions;

namespace MailAssistant.Api.Authentication;

public sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal Principal =>
        httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal();

    public bool IsAuthenticated => Principal.Identity?.IsAuthenticated == true;

    public string Subject => GetRequiredClaim("sub");

    public string? Email => Principal.FindFirstValue("email");

    public string DisplayName =>
        Principal.FindFirstValue("name")
        ?? Principal.FindFirstValue("preferred_username")
        ?? Email
        ?? Subject;

    private string GetRequiredClaim(string claimType)
    {
        return Principal.FindFirstValue(claimType)
            ?? throw new InvalidOperationException(
                $"Authenticated user is missing the '{claimType}' claim.");
    }
}
