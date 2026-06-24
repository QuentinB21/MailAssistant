using MailAssistant.Domain.Identity;
using Xunit;

namespace MailAssistant.UnitTests.Identity;

public sealed class ApplicationUserTests
{
    [Fact]
    public void SynchronizeDoesNotModifyUnchangedUser()
    {
        var now = new DateTimeOffset(2026, 6, 24, 12, 0, 0, TimeSpan.Zero);
        var user = ApplicationUser.Create(
            "subject",
            "user@test.local",
            "Test User",
            now);

        var changed = user.Synchronize(
            "USER@test.local",
            "Test User",
            now.AddMinutes(1));

        Assert.False(changed);
        Assert.Equal(now, user.UpdatedAt);
    }

    [Fact]
    public void SynchronizeUpdatesChangedClaims()
    {
        var now = new DateTimeOffset(2026, 6, 24, 12, 0, 0, TimeSpan.Zero);
        var later = now.AddMinutes(1);
        var user = ApplicationUser.Create(
            "subject",
            "user@test.local",
            "Test User",
            now);

        var changed = user.Synchronize(
            "new@test.local",
            "Updated User",
            later);

        Assert.True(changed);
        Assert.Equal("new@test.local", user.Email);
        Assert.Equal("Updated User", user.DisplayName);
        Assert.Equal(later, user.UpdatedAt);
    }

    [Fact]
    public void CreateNormalizesEmailForStableLookup()
    {
        var user = ApplicationUser.Create(
            "subject",
            " User@Test.Local ",
            "Test User",
            new DateTimeOffset(2026, 6, 24, 12, 0, 0, TimeSpan.Zero));

        Assert.Equal("user@test.local", user.Email);
    }
}
