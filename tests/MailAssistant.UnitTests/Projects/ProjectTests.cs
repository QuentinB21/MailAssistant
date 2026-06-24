using MailAssistant.Domain.Projects;
using Xunit;

namespace MailAssistant.UnitTests.Projects;

public sealed class ProjectTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 6, 24, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateTrimsProjectValues()
    {
        var project = Project.Create(
            Guid.NewGuid(),
            "  Apollo  ",
            "  Projet Apollo  ",
            "  Programme spatial  ",
            Now);

        Assert.Equal("Apollo", project.Name);
        Assert.Equal("Projet Apollo", project.ClassificationTargetName);
        Assert.Equal("Programme spatial", project.Description);
        Assert.True(project.IsActive);
    }

    [Fact]
    public void AddAliasRejectsDuplicatesIgnoringCase()
    {
        var project = Project.Create(
            Guid.NewGuid(),
            "Apollo",
            "Apollo",
            null,
            Now);

        project.AddAlias("NASA", Now);

        var exception = Assert.Throws<InvalidOperationException>(
            () => project.AddAlias("nasa", Now));

        Assert.Equal("This alias already exists on the project.", exception.Message);
    }

    [Fact]
    public void UpdateAliasCanDisableIt()
    {
        var project = Project.Create(
            Guid.NewGuid(),
            "Apollo",
            "Apollo",
            null,
            Now);
        var alias = project.AddAlias("NASA", Now);
        var later = Now.AddMinutes(1);

        project.UpdateAlias(alias.Id, "NASA Program", false, later);

        Assert.Equal("NASA Program", alias.Value);
        Assert.False(alias.IsActive);
        Assert.Equal(later, project.UpdatedAt);
    }
}
