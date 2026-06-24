using MailAssistant.Application.Identity;
using MailAssistant.Domain.Identity;

namespace MailAssistant.Api;

public static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
                "/api/me",
                async (
                    CurrentUserService service,
                    CancellationToken cancellationToken) =>
                {
                    var user = await service.GetOrCreateAsync(cancellationToken);
                    return Results.Ok(new
                    {
                        user.Id,
                        user.Subject,
                        user.Email,
                        user.DisplayName,
                    });
                })
            .RequireAuthorization()
            .WithTags("Identity");

        var group = endpoints
            .MapGroup("/api/organizations/{organizationId:guid}/members")
            .RequireAuthorization()
            .WithTags("Memberships");

        group.MapGet(
            "/",
            async (
                Guid organizationId,
                MembershipService service,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(
                    await service.ListAsync(organizationId, cancellationToken));
            });

        group.MapPut(
            "/by-email",
            async (
                Guid organizationId,
                UpsertMembershipRequest request,
                MembershipService service,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(
                    await service.AddOrUpdateAsync(
                        organizationId,
                        request.Email,
                        request.Role,
                        cancellationToken));
            });

        return endpoints;
    }

    public sealed record UpsertMembershipRequest(string Email, OrganizationRole Role);
}
