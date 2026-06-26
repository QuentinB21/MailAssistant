using MailAssistant.Application.Abstractions;
using MailAssistant.Domain.MailAccounts;
using Microsoft.EntityFrameworkCore;

namespace MailAssistant.Infrastructure.Persistence;

internal sealed class OAuthCredentialRepository(MailAssistantDbContext dbContext)
    : IOAuthCredentialRepository
{
    public Task<OAuthCredential?> GetAsync(
        Guid mailAccountId,
        CancellationToken cancellationToken)
    {
        return dbContext.OAuthCredentials.SingleOrDefaultAsync(
            credential => credential.MailAccountId == mailAccountId,
            cancellationToken);
    }

    public async Task AddAsync(
        OAuthCredential credential,
        CancellationToken cancellationToken)
    {
        await dbContext.OAuthCredentials.AddAsync(credential, cancellationToken);
    }

    public void Remove(OAuthCredential credential)
    {
        dbContext.OAuthCredentials.Remove(credential);
    }
}
