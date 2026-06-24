using MailAssistant.Application.Abstractions;
using MailAssistant.Application.Common;
using MailAssistant.Domain.Identity;

namespace MailAssistant.Application.Identity;

public sealed class MembershipService(
    OrganizationAccessService access,
    IUserRepository users,
    IMembershipRepository memberships,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
{
    public async Task<IReadOnlyCollection<MembershipResponse>> ListAsync(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        await access.RequireAsync(
            organizationId,
            OrganizationRole.Admin,
            cancellationToken);

        var results = await memberships.ListForOrganizationAsync(
            organizationId,
            cancellationToken);

        return results
            .Select(result => Map(result.Membership, result.User))
            .ToArray();
    }

    public async Task<MembershipResponse> AddOrUpdateAsync(
        Guid organizationId,
        string email,
        OrganizationRole role,
        CancellationToken cancellationToken)
    {
        await access.RequireAsync(
            organizationId,
            OrganizationRole.Owner,
            cancellationToken);

        if (role == OrganizationRole.Owner)
        {
            throw new AccessDeniedException(
                "Ownership transfer is not supported in this iteration.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        var user = await users.GetByEmailAsync(email.Trim(), cancellationToken)
            ?? throw new KeyNotFoundException(
                "The user must sign in once before being added to an organization.");

        var membership = await memberships.GetAsync(
            organizationId,
            user.Id,
            cancellationToken);

        if (membership is null)
        {
            membership = OrganizationMembership.Create(
                organizationId,
                user.Id,
                role,
                timeProvider.GetUtcNow());
            await memberships.AddAsync(membership, cancellationToken);
        }
        else
        {
            membership.ChangeRole(role, timeProvider.GetUtcNow());
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(membership, user);
    }

    private static MembershipResponse Map(
        OrganizationMembership membership,
        ApplicationUser user)
    {
        return new MembershipResponse(
            user.Id,
            user.Email,
            user.DisplayName,
            membership.Role,
            membership.CreatedAt,
            membership.UpdatedAt);
    }
}
