using MailAssistant.Domain.Matching;
using MailAssistant.Domain.Organizations;
using Xunit;

namespace MailAssistant.UnitTests.Organizations;

public sealed class OrganizationSettingsTests
{
    [Fact]
    public void CreateUsesSafeDefaults()
    {
        var settings = OrganizationSettings.Create(Guid.NewGuid());

        Assert.Equal(
            MultipleMatchBehavior.MarkAsConflict,
            settings.MultipleMatchBehavior);
        Assert.Equal(NoMatchBehavior.Ignore, settings.NoMatchBehavior);
        Assert.False(settings.ArchiveGmailAfterClassification);
    }

    [Fact]
    public void UpdatePersistsClassificationPreferences()
    {
        var settings = OrganizationSettings.Create(Guid.NewGuid());

        settings.Update(
            MultipleMatchBehavior.MarkAsConflict,
            NoMatchBehavior.Ignore,
            true);

        Assert.True(settings.ArchiveGmailAfterClassification);
    }
}
