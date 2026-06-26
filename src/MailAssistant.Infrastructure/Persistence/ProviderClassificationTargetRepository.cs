using MailAssistant.Application.Abstractions;
using MailAssistant.Domain.MailAccounts;
using Microsoft.EntityFrameworkCore;

namespace MailAssistant.Infrastructure.Persistence;

internal sealed class ProviderClassificationTargetRepository(MailAssistantDbContext dbContext)
    : IProviderClassificationTargetRepository
{
    public Task<ProviderClassificationTarget?> GetAsync(
        Guid mailAccountId,
        Guid projectId,
        CancellationToken cancellationToken)
    {
        return dbContext.ProviderClassificationTargets.SingleOrDefaultAsync(
            target => target.MailAccountId == mailAccountId
                && target.ProjectId == projectId,
            cancellationToken);
    }

    public async Task AddAsync(
        ProviderClassificationTarget target,
        CancellationToken cancellationToken)
    {
        await dbContext.ProviderClassificationTargets.AddAsync(target, cancellationToken);
    }
}
