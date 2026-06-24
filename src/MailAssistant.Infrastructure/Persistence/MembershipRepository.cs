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

    public async Task<IReadOnlyCollection<MembershipDetails>> ListForOrganizationAsync(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        return await (
            from membership in dbContext.OrganizationMemberships.AsNoTracking()
            join user in dbContext.Users.AsNoTracking()
                on membership.UserId equals user.Id
            where membership.OrganizationId == organizationId
            orderby membership.Role descending, membership.CreatedAt
            select new MembershipDetails(membership, user))
            .ToArrayAsync(cancellationToken);
    }

    public async Task AddAsync(
        OrganizationMembership membership,
        CancellationToken cancellationToken)
    {
        await dbContext.OrganizationMemberships.AddAsync(membership, cancellationToken);
    }
}
