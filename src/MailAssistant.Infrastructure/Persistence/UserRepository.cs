using MailAssistant.Application.Abstractions;
using MailAssistant.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace MailAssistant.Infrastructure.Persistence;

internal sealed class UserRepository(MailAssistantDbContext dbContext) : IUserRepository
{
    public Task<ApplicationUser?> GetBySubjectAsync(
        string subject,
        CancellationToken cancellationToken)
    {
        return dbContext.Users.SingleOrDefaultAsync(
            user => user.Subject == subject,
            cancellationToken);
    }

    public Task<ApplicationUser?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        return dbContext.Users.SingleOrDefaultAsync(
            user => user.Email != null && EF.Functions.ILike(user.Email, email),
            cancellationToken);
    }

    public Task<ApplicationUser?> GetAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);
    }

    public async Task AddAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        await dbContext.Users.AddAsync(user, cancellationToken);
    }
}
