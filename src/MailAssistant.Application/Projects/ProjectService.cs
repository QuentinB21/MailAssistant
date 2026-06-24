using MailAssistant.Application.Abstractions;
using MailAssistant.Application.Identity;
using MailAssistant.Domain.Identity;
using MailAssistant.Domain.Matching;
using MailAssistant.Domain.Projects;

namespace MailAssistant.Application.Projects;

public sealed class ProjectService(
    IOrganizationRepository organizations,
    IProjectRepository projects,
    OrganizationAccessService access,
    IUnitOfWork unitOfWork,
    ISubjectNormalizer subjectNormalizer,
    IMatchingStrategy matchingStrategy,
    IConflictResolutionPolicy conflictResolutionPolicy,
    TimeProvider timeProvider)
{
    public async Task<IReadOnlyCollection<ProjectResponse>> ListAsync(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        await access.RequireAsync(
            organizationId,
            OrganizationRole.Member,
            cancellationToken);
        await EnsureOrganizationExistsAsync(organizationId, cancellationToken);
        var results = await projects.ListAsync(organizationId, cancellationToken);
        return results.Select(Map).ToArray();
    }

    public async Task<ProjectResponse> GetAsync(
        Guid organizationId,
        Guid projectId,
        CancellationToken cancellationToken)
    {
        await access.RequireAsync(
            organizationId,
            OrganizationRole.Member,
            cancellationToken);
        return Map(await GetProjectAsync(organizationId, projectId, cancellationToken));
    }

    public async Task<ProjectResponse> CreateAsync(
        Guid organizationId,
        CreateProjectCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        await access.RequireAsync(
            organizationId,
            OrganizationRole.Admin,
            cancellationToken);
        await EnsureOrganizationExistsAsync(organizationId, cancellationToken);
        await EnsureProjectNameIsAvailableAsync(
            organizationId,
            command.Name,
            null,
            cancellationToken);

        var project = Project.Create(
            organizationId,
            command.Name,
            command.ClassificationTargetName,
            command.Description,
            timeProvider.GetUtcNow());

        await projects.AddAsync(project, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(project);
    }

    public async Task<ProjectResponse> UpdateAsync(
        Guid organizationId,
        Guid projectId,
        UpdateProjectCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        await access.RequireAsync(
            organizationId,
            OrganizationRole.Admin,
            cancellationToken);
        var project = await GetProjectAsync(organizationId, projectId, cancellationToken);
        await EnsureProjectNameIsAvailableAsync(
            organizationId,
            command.Name,
            projectId,
            cancellationToken);

        project.Update(
            command.Name,
            command.ClassificationTargetName,
            command.Description,
            command.IsActive,
            timeProvider.GetUtcNow());

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(project);
    }

    public async Task DeleteAsync(
        Guid organizationId,
        Guid projectId,
        CancellationToken cancellationToken)
    {
        await access.RequireAsync(
            organizationId,
            OrganizationRole.Admin,
            cancellationToken);
        var project = await GetProjectAsync(organizationId, projectId, cancellationToken);
        projects.Remove(project);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProjectAliasResponse> AddAliasAsync(
        Guid organizationId,
        Guid projectId,
        string value,
        CancellationToken cancellationToken)
    {
        await access.RequireAsync(
            organizationId,
            OrganizationRole.Admin,
            cancellationToken);
        var project = await GetProjectAsync(organizationId, projectId, cancellationToken);
        var alias = project.AddAlias(value, timeProvider.GetUtcNow());

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(alias);
    }

    public async Task<ProjectAliasResponse> UpdateAliasAsync(
        Guid organizationId,
        Guid projectId,
        Guid aliasId,
        UpdateAliasCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        await access.RequireAsync(
            organizationId,
            OrganizationRole.Admin,
            cancellationToken);
        var project = await GetProjectAsync(organizationId, projectId, cancellationToken);
        project.UpdateAlias(aliasId, command.Value, command.IsActive, timeProvider.GetUtcNow());

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(project.Aliases.Single(alias => alias.Id == aliasId));
    }

    public async Task DeleteAliasAsync(
        Guid organizationId,
        Guid projectId,
        Guid aliasId,
        CancellationToken cancellationToken)
    {
        await access.RequireAsync(
            organizationId,
            OrganizationRole.Admin,
            cancellationToken);
        var project = await GetProjectAsync(organizationId, projectId, cancellationToken);
        project.RemoveAlias(aliasId, timeProvider.GetUtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<MatchingTestResponse> TestSubjectAsync(
        Guid organizationId,
        string subject,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(subject);
        await access.RequireAsync(
            organizationId,
            OrganizationRole.Member,
            cancellationToken);
        await EnsureOrganizationExistsAsync(organizationId, cancellationToken);

        var organizationProjects = await projects.ListAsync(organizationId, cancellationToken);
        var definitions = organizationProjects
            .Select(project => new ProjectMatchDefinition(
                project.Id,
                project.Name,
                project.IsActive,
                project.Aliases
                    .Select(alias => new ProjectAliasMatchDefinition(alias.Value, alias.IsActive))
                    .ToArray()))
            .ToArray();

        var normalizedSubject = subjectNormalizer.Normalize(subject);
        var matches = matchingStrategy.FindMatches(normalizedSubject, definitions);
        var decision = conflictResolutionPolicy.Resolve(normalizedSubject, matches);

        return new MatchingTestResponse(
            decision.Outcome,
            decision.NormalizedSubject,
            decision.SelectedProjectId,
            decision.Matches
                .Select(match => new ProjectMatchResponse(
                    match.ProjectId,
                    match.ProjectName,
                    match.MatchedValue,
                    match.Source))
                .ToArray());
    }

    private async Task EnsureOrganizationExistsAsync(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        if (organizationId == Guid.Empty
            || await organizations.GetAsync(organizationId, cancellationToken) is null)
        {
            throw new KeyNotFoundException("Organization not found.");
        }
    }

    private async Task<Project> GetProjectAsync(
        Guid organizationId,
        Guid projectId,
        CancellationToken cancellationToken)
    {
        if (organizationId == Guid.Empty || projectId == Guid.Empty)
        {
            throw new KeyNotFoundException("Project not found.");
        }

        return await projects.GetAsync(organizationId, projectId, cancellationToken)
            ?? throw new KeyNotFoundException("Project not found.");
    }

    private async Task EnsureProjectNameIsAvailableAsync(
        Guid organizationId,
        string name,
        Guid? excludedProjectId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (await projects.NameExistsAsync(
            organizationId,
            name.Trim(),
            excludedProjectId,
            cancellationToken))
        {
            throw new InvalidOperationException(
                "A project with this name already exists in the organization.");
        }
    }

    private static ProjectResponse Map(Project project)
    {
        return new ProjectResponse(
            project.Id,
            project.OrganizationId,
            project.Name,
            project.Description,
            project.IsActive,
            project.ClassificationTargetName,
            project.CreatedAt,
            project.UpdatedAt,
            project.Aliases.Select(Map).ToArray());
    }

    private static ProjectAliasResponse Map(ProjectAlias alias)
    {
        return new ProjectAliasResponse(
            alias.Id,
            alias.Value,
            alias.IsActive,
            alias.CreatedAt,
            alias.UpdatedAt);
    }
}
