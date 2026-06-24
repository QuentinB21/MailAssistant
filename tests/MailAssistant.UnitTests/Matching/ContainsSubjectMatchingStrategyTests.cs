using MailAssistant.Domain.Matching;
using Xunit;

namespace MailAssistant.UnitTests.Matching;

public sealed class ContainsSubjectMatchingStrategyTests
{
    private readonly SubjectNormalizer _normalizer = new();

    [Fact]
    public void FindMatchesUsesProjectNameAndAlias()
    {
        var projectId = Guid.NewGuid();
        var strategy = new ContainsSubjectMatchingStrategy(_normalizer);
        var projects = new[]
        {
            new ProjectMatchDefinition(
                projectId,
                "Projet Émeraude",
                true,
                [new ProjectAliasMatchDefinition("PEM", true)]),
        };

        var matches = strategy.FindMatches(
            _normalizer.Normalize("RE: Compte rendu PEM"),
            projects);

        var match = Assert.Single(matches);
        Assert.Equal(projectId, match.ProjectId);
        Assert.Equal(MatchSource.Alias, match.Source);
    }

    [Fact]
    public void FindMatchesDoesNotMatchInsideAnotherWord()
    {
        var strategy = new ContainsSubjectMatchingStrategy(_normalizer);
        var projects = new[]
        {
            new ProjectMatchDefinition(
                Guid.NewGuid(),
                "CRM",
                true,
                []),
        };

        var matches = strategy.FindMatches(
            _normalizer.Normalize("Migration ACRMTool"),
            projects);

        Assert.Empty(matches);
    }

    [Fact]
    public void FindMatchesIgnoresDisabledProjectsAndAliases()
    {
        var strategy = new ContainsSubjectMatchingStrategy(_normalizer);
        var projects = new[]
        {
            new ProjectMatchDefinition(
                Guid.NewGuid(),
                "Projet Alpha",
                false,
                [new ProjectAliasMatchDefinition("Alpha", true)]),
            new ProjectMatchDefinition(
                Guid.NewGuid(),
                "Projet Beta",
                true,
                [new ProjectAliasMatchDefinition("Beta", false)]),
        };

        var matches = strategy.FindMatches(
            _normalizer.Normalize("Alpha et Beta"),
            projects);

        Assert.Empty(matches);
    }

    [Fact]
    public void FindMatchesReturnsOneMostSpecificMatchPerProject()
    {
        var projectId = Guid.NewGuid();
        var strategy = new ContainsSubjectMatchingStrategy(_normalizer);
        var projects = new[]
        {
            new ProjectMatchDefinition(
                projectId,
                "Apollo",
                true,
                [new ProjectAliasMatchDefinition("Apollo Europe", true)]),
        };

        var matches = strategy.FindMatches(
            _normalizer.Normalize("Apollo Europe - suivi Apollo"),
            projects);

        var match = Assert.Single(matches);
        Assert.Equal("Apollo Europe", match.MatchedValue);
    }
}
