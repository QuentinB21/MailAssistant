using MailAssistant.Domain.Matching;
using Xunit;

namespace MailAssistant.UnitTests.Matching;

public sealed class ConflictResolutionPolicyTests
{
    private readonly ConflictResolutionPolicy _policy = new();

    [Fact]
    public void ResolveSelectsTheOnlyProject()
    {
        var projectId = Guid.NewGuid();
        var matches = new[]
        {
            new ProjectMatch(projectId, "Apollo", "Apollo", MatchSource.ProjectName),
        };

        var decision = _policy.Resolve("apollo", matches);

        Assert.Equal(MatchOutcome.Matched, decision.Outcome);
        Assert.Equal(projectId, decision.SelectedProjectId);
    }

    [Fact]
    public void ResolveMarksMultipleProjectsAsConflict()
    {
        var matches = new[]
        {
            new ProjectMatch(Guid.NewGuid(), "Apollo", "Apollo", MatchSource.ProjectName),
            new ProjectMatch(Guid.NewGuid(), "Gemini", "Gemini", MatchSource.ProjectName),
        };

        var decision = _policy.Resolve("apollo gemini", matches);

        Assert.Equal(MatchOutcome.Conflict, decision.Outcome);
        Assert.Null(decision.SelectedProjectId);
    }

    [Fact]
    public void ResolveReturnsNoMatchForEmptyResults()
    {
        var decision = _policy.Resolve("information generale", []);

        Assert.Equal(MatchOutcome.NoMatch, decision.Outcome);
        Assert.Null(decision.SelectedProjectId);
    }
}
