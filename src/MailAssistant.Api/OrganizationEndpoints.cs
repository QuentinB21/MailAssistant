using MailAssistant.Application.Organizations;

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

        return endpoints;
    }

    public sealed record CreateOrganizationRequest(string Name);
}
