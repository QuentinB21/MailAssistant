using MailAssistant.Application.Abstractions;
using MailAssistant.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace MailAssistant.Infrastructure.Persistence;

internal sealed class MembershipRepository(MailAssistantDbContext dbContext)
    : IMembershipRepository
{
    public Task<OrganizationMembership?> GetAsync(
        Guid organizationId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return dbContext.OrganizationMemberships.SingleOrDefaultAsync(
            membership => membership.OrganizationId == organizationId
                && membership.UserId == userId,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<OrganizationMembership>> ListForOrganizationAsync(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        return await dbContext.OrganizationMemberships
            .AsNoTracking()
            .Where(membership => membership.OrganizationId == organizationId)
            .OrderByDescending(membership => membership.Role)
            .ThenBy(membership => membership.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }

    public async Task AddAsync(
        OrganizationMembership membership,
        CancellationToken cancellationToken)
    {
        await dbContext.OrganizationMemberships.AddAsync(membership, cancellationToken);
    }
}
