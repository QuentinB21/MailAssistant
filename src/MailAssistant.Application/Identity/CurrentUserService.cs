using MailAssistant.Application.Abstractions;
using MailAssistant.Application.Common;
using MailAssistant.Domain.Identity;

namespace MailAssistant.Application.Identity;

public sealed class CurrentUserService(
    ICurrentUser currentUser,
    IUserRepository users,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
{
    public async Task<ApplicationUser> GetOrCreateAsync(
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated)
        {
            throw new AuthenticationRequiredException();
        }

        var user = await users.GetBySubjectAsync(currentUser.Subject, cancellationToken);
        var hasChanges = false;
        if (user is null)
        {
            user = ApplicationUser.Create(
                currentUser.Subject,
                currentUser.Email,
                currentUser.DisplayName,
                timeProvider.GetUtcNow());
            await users.AddAsync(user, cancellationToken);
            hasChanges = true;
        }
        else
        {
            hasChanges = user.Synchronize(
                currentUser.Email,
                currentUser.DisplayName,
                timeProvider.GetUtcNow());
        }

        if (hasChanges)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return user;
    }
}
