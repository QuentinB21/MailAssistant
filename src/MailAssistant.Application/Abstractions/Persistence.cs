using MailAssistant.Domain.Identity;
using MailAssistant.Domain.Organizations;
using MailAssistant.Domain.Projects;

namespace MailAssistant.Application.Abstractions;

public sealed record OrganizationAccessRecord(
    Organization Organization,
    OrganizationRole Role);

public interface IOrganizationRepository
{
    Task<IReadOnlyCollection<OrganizationAccessRecord>> ListForUserAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<Organization?> GetAsync(Guid organizationId, CancellationToken cancellationToken);

    Task AddAsync(Organization organization, CancellationToken cancellationToken);
}

public interface IProjectRepository
{
    Task<IReadOnlyCollection<Project>> ListAsync(
        Guid organizationId,
        CancellationToken cancellationToken);

    Task<Project?> GetAsync(
        Guid organizationId,
        Guid projectId,
        CancellationToken cancellationToken);

    Task<bool> NameExistsAsync(
        Guid organizationId,
        string name,
        Guid? excludedProjectId,
        CancellationToken cancellationToken);

    Task AddAsync(Project project, CancellationToken cancellationToken);

    void Remove(Project project);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
