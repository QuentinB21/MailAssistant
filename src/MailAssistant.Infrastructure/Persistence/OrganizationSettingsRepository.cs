using MailAssistant.Application.Abstractions;
using MailAssistant.Domain.Organizations;
using Microsoft.EntityFrameworkCore;

namespace MailAssistant.Infrastructure.Persistence;

internal sealed class OrganizationSettingsRepository(MailAssistantDbContext dbContext)
    : IOrganizationSettingsRepository
{
    public Task<OrganizationSettings?> GetAsync(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        return dbContext.OrganizationSettings.SingleOrDefaultAsync(
            settings => settings.OrganizationId == organizationId,
            cancellationToken);
    }
}
