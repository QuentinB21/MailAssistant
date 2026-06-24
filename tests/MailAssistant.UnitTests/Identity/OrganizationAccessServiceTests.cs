using MailAssistant.Application.Abstractions;
using MailAssistant.Application.Common;
using MailAssistant.Application.Identity;
using MailAssistant.Domain.Identity;
using Xunit;

namespace MailAssistant.UnitTests.Identity;

public sealed class OrganizationAccessServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 6, 24, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task RequireAsyncAllowsSufficientRole()
    {
        var organizationId = Guid.NewGuid();
        var user = ApplicationUser.Create("subject", "admin@test.local", "Admin", Now);
        var service = CreateService(
            user,
            OrganizationMembership.Create(
                organizationId,
                user.Id,
                OrganizationRole.Admin,
                Now));

        var membership = await service.RequireAsync(
            organizationId,
            OrganizationRole.Admin,
            CancellationToken.None);

        Assert.Equal(OrganizationRole.Admin, membership.Role);
    }

    [Fact]
    public async Task RequireAsyncRejectsInsufficientRole()
    {
        var organizationId = Guid.NewGuid();
        var user = ApplicationUser.Create("subject", "member@test.local", "Member", Now);
        var service = CreateService(
            user,
            OrganizationMembership.Create(
                organizationId,
                user.Id,
                OrganizationRole.Member,
                Now));

        await Assert.ThrowsAsync<AccessDeniedException>(
            () => service.RequireAsync(
                organizationId,
                OrganizationRole.Admin,
                CancellationToken.None));
    }

    [Fact]
    public async Task RequireAsyncRejectsUserOutsideOrganization()
    {
        var user = ApplicationUser.Create("subject", "outsider@test.local", "Outsider", Now);
        var service = CreateService(user, null);

        await Assert.ThrowsAsync<AccessDeniedException>(
            () => service.RequireAsync(
                Guid.NewGuid(),
                OrganizationRole.Member,
                CancellationToken.None));
    }

    private static OrganizationAccessService CreateService(
        ApplicationUser user,
        OrganizationMembership? membership)
    {
        var currentUser = new FakeCurrentUser(user);
        var userRepository = new FakeUserRepository(user);
        var currentUserService = new CurrentUserService(
            currentUser,
            userRepository,
            new FakeUnitOfWork(),
            TimeProvider.System);

        return new OrganizationAccessService(
            currentUserService,
            new FakeMembershipRepository(user, membership));
    }

    private sealed class FakeCurrentUser(ApplicationUser user) : ICurrentUser
    {
        public bool IsAuthenticated => true;

        public string Subject => user.Subject;

        public string? Email => user.Email;

        public string DisplayName => user.DisplayName;
    }

    private sealed class FakeUserRepository(ApplicationUser user) : IUserRepository
    {
        public Task<ApplicationUser?> GetBySubjectAsync(
            string subject,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<ApplicationUser?>(user);
        }

        public Task<ApplicationUser?> GetByEmailAsync(
            string email,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<ApplicationUser?>(user);
        }

        public Task<ApplicationUser?> GetAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<ApplicationUser?>(user);
        }

        public Task AddAsync(
            ApplicationUser newUser,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeMembershipRepository(
        ApplicationUser user,
        OrganizationMembership? membership)
        : IMembershipRepository
    {
        public Task<OrganizationMembership?> GetAsync(
            Guid organizationId,
            Guid userId,
            CancellationToken cancellationToken)
        {
            var result = membership?.OrganizationId == organizationId
                && membership.UserId == userId
                    ? membership
                    : null;
            return Task.FromResult(result);
        }

        public Task<IReadOnlyCollection<MembershipDetails>> ListForOrganizationAsync(
            Guid organizationId,
            CancellationToken cancellationToken)
        {
            IReadOnlyCollection<MembershipDetails> results = membership is null
                ? []
                : [new MembershipDetails(membership, user)];
            return Task.FromResult(results);
        }

        public Task AddAsync(
            OrganizationMembership newMembership,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }
}
