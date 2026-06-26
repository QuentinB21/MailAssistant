using MailAssistant.Domain.Identity;
using MailAssistant.Domain.MailAccounts;
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

public interface IOrganizationSettingsRepository
{
    Task<OrganizationSettings?> GetAsync(
        Guid organizationId,
        CancellationToken cancellationToken);
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

public interface IMailAccountRepository
{
    Task<IReadOnlyCollection<MailAccount>> ListAsync(
        Guid organizationId,
        CancellationToken cancellationToken);

    Task<MailAccount?> GetAsync(
        Guid organizationId,
        Guid mailAccountId,
        CancellationToken cancellationToken);

    Task<MailAccount?> GetByProviderAddressAsync(
        Guid organizationId,
        MailProvider provider,
        string emailAddress,
        CancellationToken cancellationToken);

    Task AddAsync(MailAccount account, CancellationToken cancellationToken);

    void Remove(MailAccount account);
}

public interface IOAuthCredentialRepository
{
    Task<OAuthCredential?> GetAsync(
        Guid mailAccountId,
        CancellationToken cancellationToken);

    Task AddAsync(OAuthCredential credential, CancellationToken cancellationToken);

    void Remove(OAuthCredential credential);
}

public interface IProviderClassificationTargetRepository
{
    Task<ProviderClassificationTarget?> GetAsync(
        Guid mailAccountId,
        Guid projectId,
        CancellationToken cancellationToken);

    Task AddAsync(
        ProviderClassificationTarget target,
        CancellationToken cancellationToken);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
