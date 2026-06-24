using MailAssistant.Application.Abstractions;
using MailAssistant.Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace MailAssistant.Infrastructure.Persistence;

internal sealed class ProjectRepository(MailAssistantDbContext dbContext) : IProjectRepository
{
    public async Task<IReadOnlyCollection<Project>> ListAsync(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Projects
            .AsNoTracking()
            .Include(project => project.Aliases)
            .Where(project => project.OrganizationId == organizationId)
            .OrderBy(project => project.Name)
            .ToArrayAsync(cancellationToken);
    }

    public Task<Project?> GetAsync(
        Guid organizationId,
        Guid projectId,
        CancellationToken cancellationToken)
    {
        return dbContext.Projects
            .Include(project => project.Aliases)
            .SingleOrDefaultAsync(
                project => project.OrganizationId == organizationId
                    && project.Id == projectId,
                cancellationToken);
    }

    public Task<bool> NameExistsAsync(
        Guid organizationId,
        string name,
        Guid? excludedProjectId,
        CancellationToken cancellationToken)
    {
        var escapedName = EscapeLikePattern(name);

        return dbContext.Projects.AnyAsync(
            project => project.OrganizationId == organizationId
                && (excludedProjectId == null || project.Id != excludedProjectId)
                && EF.Functions.ILike(project.Name, escapedName, "\\"),
            cancellationToken);
    }

    public async Task AddAsync(Project project, CancellationToken cancellationToken)
    {
        await dbContext.Projects.AddAsync(project, cancellationToken);
    }

    public void Remove(Project project)
    {
        dbContext.Projects.Remove(project);
    }

    private static string EscapeLikePattern(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
    }
}
