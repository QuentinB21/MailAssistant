using MailAssistant.Application.Projects;

namespace MailAssistant.Api;

public static class ProjectEndpoints
{
    public static IEndpointRouteBuilder MapProjectEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/organizations/{organizationId:guid}")
            .RequireAuthorization()
            .WithTags("Projects");

        group.MapGet(
            "/projects",
            async (
                Guid organizationId,
                ProjectService service,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(await service.ListAsync(organizationId, cancellationToken));
            });

        group.MapGet(
            "/projects/{projectId:guid}",
            async (
                Guid organizationId,
                Guid projectId,
                ProjectService service,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(
                    await service.GetAsync(organizationId, projectId, cancellationToken));
            });

        group.MapPost(
            "/projects",
            async (
                Guid organizationId,
                CreateProjectRequest request,
                ProjectService service,
                CancellationToken cancellationToken) =>
            {
                var project = await service.CreateAsync(
                    organizationId,
                    new CreateProjectCommand(
                        request.Name,
                        request.ClassificationTargetName,
                        request.Description),
                    cancellationToken);

                return Results.Created(
                    $"/api/organizations/{organizationId}/projects/{project.Id}",
                    project);
            });

        group.MapPut(
            "/projects/{projectId:guid}",
            async (
                Guid organizationId,
                Guid projectId,
                UpdateProjectRequest request,
                ProjectService service,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(
                    await service.UpdateAsync(
                        organizationId,
                        projectId,
                        new UpdateProjectCommand(
                            request.Name,
                            request.ClassificationTargetName,
                            request.Description,
                            request.IsActive),
                        cancellationToken));
            });

        group.MapDelete(
            "/projects/{projectId:guid}",
            async (
                Guid organizationId,
                Guid projectId,
                ProjectService service,
                CancellationToken cancellationToken) =>
            {
                await service.DeleteAsync(organizationId, projectId, cancellationToken);
                return Results.NoContent();
            });

        group.MapPost(
            "/projects/{projectId:guid}/aliases",
            async (
                Guid organizationId,
                Guid projectId,
                CreateAliasRequest request,
                ProjectService service,
                CancellationToken cancellationToken) =>
            {
                var alias = await service.AddAliasAsync(
                    organizationId,
                    projectId,
                    request.Value,
                    cancellationToken);

                return Results.Created(
                    $"/api/organizations/{organizationId}/projects/{projectId}/aliases/{alias.Id}",
                    alias);
            });

        group.MapPut(
            "/projects/{projectId:guid}/aliases/{aliasId:guid}",
            async (
                Guid organizationId,
                Guid projectId,
                Guid aliasId,
                UpdateAliasRequest request,
                ProjectService service,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(
                    await service.UpdateAliasAsync(
                        organizationId,
                        projectId,
                        aliasId,
                        new UpdateAliasCommand(request.Value, request.IsActive),
                        cancellationToken));
            });

        group.MapDelete(
            "/projects/{projectId:guid}/aliases/{aliasId:guid}",
            async (
                Guid organizationId,
                Guid projectId,
                Guid aliasId,
                ProjectService service,
                CancellationToken cancellationToken) =>
            {
                await service.DeleteAliasAsync(
                    organizationId,
                    projectId,
                    aliasId,
                    cancellationToken);

                return Results.NoContent();
            });

        group.MapPost(
            "/matching-tests",
            async (
                Guid organizationId,
                MatchingTestRequest request,
                SubjectMatchingService service,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(
                    await service.TestSubjectAsync(
                        organizationId,
                        request.Subject,
                        cancellationToken));
            });

        return endpoints;
    }

    public sealed record CreateProjectRequest(
        string Name,
        string ClassificationTargetName,
        string? Description);

    public sealed record UpdateProjectRequest(
        string Name,
        string ClassificationTargetName,
        string? Description,
        bool IsActive);

    public sealed record CreateAliasRequest(string Value);

    public sealed record UpdateAliasRequest(string Value, bool IsActive);

    public sealed record MatchingTestRequest(string Subject);
}
