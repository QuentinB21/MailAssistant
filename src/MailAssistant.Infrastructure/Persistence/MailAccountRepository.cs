using MailAssistant.Application.Abstractions;
using MailAssistant.Domain.MailAccounts;
using Microsoft.EntityFrameworkCore;

namespace MailAssistant.Infrastructure.Persistence;

internal sealed class MailAccountRepository(MailAssistantDbContext dbContext)
    : IMailAccountRepository
{
    public async Task<IReadOnlyCollection<MailAccount>> ListAsync(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        return await dbContext.MailAccounts
            .Where(account => account.OrganizationId == organizationId)
            .OrderBy(account => account.EmailAddress)
            .ToArrayAsync(cancellationToken);
    }

    public Task<MailAccount?> GetAsync(
        Guid organizationId,
        Guid mailAccountId,
        CancellationToken cancellationToken)
    {
        return dbContext.MailAccounts.SingleOrDefaultAsync(
            account => account.OrganizationId == organizationId
                && account.Id == mailAccountId,
            cancellationToken);
    }

    public Task<MailAccount?> GetByProviderAddressAsync(
        Guid organizationId,
        MailProvider provider,
        string emailAddress,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = emailAddress.Trim().ToLowerInvariant();
        return dbContext.MailAccounts.SingleOrDefaultAsync(
            account => account.OrganizationId == organizationId
                && account.Provider == provider
                && account.EmailAddress == normalizedEmail,
            cancellationToken);
    }

    public async Task AddAsync(MailAccount account, CancellationToken cancellationToken)
    {
        await dbContext.MailAccounts.AddAsync(account, cancellationToken);
    }

    public void Remove(MailAccount account)
    {
        dbContext.MailAccounts.Remove(account);
    }
}
