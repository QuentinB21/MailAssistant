using MailAssistant.Application.Abstractions;
using MailAssistant.Application.Common;
using MailAssistant.Domain.Identity;

namespace MailAssistant.Application.Identity;

public sealed class OrganizationAccessService(
    CurrentUserService currentUserService,
    IMembershipRepository memberships)
{
    public async Task<OrganizationMembership> RequireAsync(
        Guid organizationId,
        OrganizationRole minimumRole,
        CancellationToken cancellationToken)
    {
        if (organizationId == Guid.Empty)
        {
            throw new KeyNotFoundException("Organization not found.");
        }

        var user = await currentUserService.GetOrCreateAsync(cancellationToken);
        var membership = await memberships.GetAsync(
            organizationId,
            user.Id,
            cancellationToken);

        if (membership is null || membership.Role < minimumRole)
        {
            throw new AccessDeniedException(
                "You do not have permission to access this organization.");
        }

        return membership;
    }
}
