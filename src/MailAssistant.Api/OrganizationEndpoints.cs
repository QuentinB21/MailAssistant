using MailAssistant.Application.Organizations;
using MailAssistant.Domain.Matching;

namespace MailAssistant.Api;

public static class OrganizationEndpoints
{
    public static IEndpointRouteBuilder MapOrganizationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/organizations")
            .RequireAuthorization()
            .WithTags("Organizations");

        group.MapGet(
            "/",
            async (
                OrganizationService service,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(await service.ListAsync(cancellationToken));
            });

        group.MapGet(
            "/{organizationId:guid}",
            async (
                Guid organizationId,
                OrganizationService service,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(
                    await service.GetAsync(organizationId, cancellationToken));
            });

        group.MapPost(
            "/",
            async (
                CreateOrganizationRequest request,
                OrganizationService service,
                CancellationToken cancellationToken) =>
            {
                var organization = await service.CreateAsync(
                    request.Name,
                    cancellationToken);

                return Results.Created(
                    $"/api/organizations/{organization.Id}",
                    organization);
            });

        group.MapGet(
            "/{organizationId:guid}/settings",
            async (
                Guid organizationId,
                OrganizationSettingsService service,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(
                    await service.GetAsync(organizationId, cancellationToken));
            });

        group.MapPut(
            "/{organizationId:guid}/settings",
            async (
                Guid organizationId,
                UpdateOrganizationSettingsRequest request,
                OrganizationSettingsService service,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(
                    await service.UpdateAsync(
                        organizationId,
                        new UpdateOrganizationSettingsCommand(
                            request.MultipleMatchBehavior,
                            request.NoMatchBehavior,
                            request.ArchiveGmailAfterClassification),
                        cancellationToken));
            });

        return endpoints;
    }

    public sealed record CreateOrganizationRequest(string Name);

    public sealed record UpdateOrganizationSettingsRequest(
        MultipleMatchBehavior MultipleMatchBehavior,
        NoMatchBehavior NoMatchBehavior,
        bool ArchiveGmailAfterClassification);
}
