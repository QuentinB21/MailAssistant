namespace MailAssistant.Domain.Matching;

public sealed record ProjectMatchDefinition(
    Guid ProjectId,
    string ProjectName,
    bool IsActive,
    IReadOnlyCollection<ProjectAliasMatchDefinition> Aliases);

public sealed record ProjectAliasMatchDefinition(string Value, bool IsActive);

public sealed record ProjectMatch(
    Guid ProjectId,
    string ProjectName,
    string MatchedValue,
    MatchSource Source);

public sealed record MatchingDecision(
    MatchOutcome Outcome,
    string NormalizedSubject,
    Guid? SelectedProjectId,
    IReadOnlyCollection<ProjectMatch> Matches);

public interface ISubjectNormalizer
{
    string Normalize(string subject);
}

public interface IMatchingStrategy
{
    IReadOnlyCollection<ProjectMatch> FindMatches(
        string normalizedSubject,
        IReadOnlyCollection<ProjectMatchDefinition> projects);
}

public interface IConflictResolutionPolicy
{
    MatchingDecision Resolve(
        string normalizedSubject,
        IReadOnlyCollection<ProjectMatch> matches);
}
