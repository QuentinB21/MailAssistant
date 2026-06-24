using MailAssistant.Application.Abstractions;
using MailAssistant.Domain.Organizations;
using Microsoft.EntityFrameworkCore;

namespace MailAssistant.Infrastructure.Persistence;

internal sealed class OrganizationRepository(MailAssistantDbContext dbContext)
    : IOrganizationRepository
{
    public async Task<IReadOnlyCollection<Organization>> ListAsync(
        CancellationToken cancellationToken)
    {
        return await dbContext.Organizations
            .AsNoTracking()
            .OrderBy(organization => organization.Name)
            .ToArrayAsync(cancellationToken);
    }

    public Task<Organization?> GetAsync(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        return dbContext.Organizations
            .AsNoTracking()
            .SingleOrDefaultAsync(
                organization => organization.Id == organizationId,
                cancellationToken);
    }

    public async Task AddAsync(
        Organization organization,
        CancellationToken cancellationToken)
    {
        await dbContext.Organizations.AddAsync(organization, cancellationToken);
        await dbContext.OrganizationSettings.AddAsync(
            OrganizationSettings.Create(organization.Id),
            cancellationToken);
    }
}
