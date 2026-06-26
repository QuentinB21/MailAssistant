using MailAssistant.Application.MailAccounts;

namespace MailAssistant.Api;

public static class GmailEndpoints
{
    public static IEndpointRouteBuilder MapGmailEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/organizations/{organizationId:guid}/mail-accounts/gmail")
            .RequireAuthorization()
            .WithTags("Gmail");

        group.MapGet(
            "/",
            async (
                Guid organizationId,
                GmailAccountService service,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(
                    await service.ListAsync(organizationId, cancellationToken));
            });

        group.MapPost(
            "/authorization",
            async (
                Guid organizationId,
                HttpRequest request,
                IConfiguration configuration,
                GmailAccountService service,
                CancellationToken cancellationToken) =>
            {
                var callbackUri = GetCallbackUri(request, configuration);
                return Results.Ok(
                    await service.CreateAuthorizationAsync(
                        organizationId,
                        callbackUri,
                        cancellationToken));
            });

        group.MapPut(
            "/{mailAccountId:guid}",
            async (
                Guid organizationId,
                Guid mailAccountId,
                UpdateGmailAccountRequest request,
                GmailAccountService service,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(
                    await service.SetAutomaticClassificationAsync(
                        organizationId,
                        mailAccountId,
                        request.IsAutomaticClassificationEnabled,
                        cancellationToken));
            });

        group.MapDelete(
            "/{mailAccountId:guid}",
            async (
                Guid organizationId,
                Guid mailAccountId,
                GmailAccountService service,
                CancellationToken cancellationToken) =>
            {
                await service.DisconnectAsync(
                    organizationId,
                    mailAccountId,
                    cancellationToken);
                return Results.NoContent();
            });

        group.MapPost(
            "/{mailAccountId:guid}/manual-classifications",
            async (
                Guid organizationId,
                Guid mailAccountId,
                ManualGmailClassificationRequest request,
                GmailAccountService service,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(
                    await service.ClassifyMessageAsync(
                        organizationId,
                        mailAccountId,
                        request.MessageId,
                        cancellationToken));
            });

        endpoints.MapGet(
            "/api/integrations/gmail/callback",
            async (
                string? code,
                string? state,
                string? error,
                HttpRequest request,
                IConfiguration configuration,
                GmailAccountService service,
                CancellationToken cancellationToken) =>
            {
                var frontendUrl = configuration["Frontend:PublicUrl"]
                    ?? "http://localhost:5173";
                if (!string.IsNullOrWhiteSpace(error))
                {
                    return Results.Redirect(
                        BuildFrontendRedirect(frontendUrl, "gmailError", error));
                }

                if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
                {
                    return Results.Redirect(
                        BuildFrontendRedirect(
                            frontendUrl,
                            "gmailError",
                            "invalid_callback"));
                }

                var callbackUri = GetCallbackUri(request, configuration);
                var result = await service.CompleteAuthorizationAsync(
                    code,
                    state,
                    callbackUri,
                    cancellationToken);
                return Results.Redirect(
                    BuildFrontendRedirect(
                        frontendUrl,
                        "gmailConnected",
                        result.OrganizationId.ToString()));
            })
            .WithTags("Gmail");

        return endpoints;
    }

    private static string GetCallbackUri(
        HttpRequest request,
        IConfiguration configuration)
    {
        return configuration["Gmail:CallbackUrl"]
            ?? $"{request.Scheme}://{request.Host}/api/integrations/gmail/callback";
    }

    private static string BuildFrontendRedirect(
        string frontendUrl,
        string parameter,
        string value)
    {
        var separator = frontendUrl.Contains('?', StringComparison.Ordinal) ? '&' : '?';
        return $"{frontendUrl}{separator}{Uri.EscapeDataString(parameter)}="
            + Uri.EscapeDataString(value);
    }

    public sealed record UpdateGmailAccountRequest(
        bool IsAutomaticClassificationEnabled);

    public sealed record ManualGmailClassificationRequest(string MessageId);
}
