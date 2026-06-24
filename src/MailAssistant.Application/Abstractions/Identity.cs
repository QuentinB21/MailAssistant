using MailAssistant.Domain.Identity;

namespace MailAssistant.Application.Abstractions;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    string Subject { get; }

    string? Email { get; }

    string DisplayName { get; }
}

public interface IUserRepository
{
    Task<ApplicationUser?> GetBySubjectAsync(
        string subject,
        CancellationToken cancellationToken);

    Task<ApplicationUser?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken);

    Task<ApplicationUser?> GetAsync(Guid userId, CancellationToken cancellationToken);

    Task AddAsync(ApplicationUser user, CancellationToken cancellationToken);
}

public interface IMembershipRepository
{
    Task<OrganizationMembership?> GetAsync(
        Guid organizationId,
        Guid userId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<OrganizationMembership>> ListForOrganizationAsync(
        Guid organizationId,
        CancellationToken cancellationToken);

    Task AddAsync(
        OrganizationMembership membership,
        CancellationToken cancellationToken);
}
