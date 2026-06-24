namespace MailAssistant.Domain.Matching;

public sealed class ConflictResolutionPolicy : IConflictResolutionPolicy
{
    public MatchingDecision Resolve(
        string normalizedSubject,
        IReadOnlyCollection<ProjectMatch> matches)
    {
        ArgumentNullException.ThrowIfNull(normalizedSubject);
        ArgumentNullException.ThrowIfNull(matches);

        return matches.Count switch
        {
            0 => new MatchingDecision(MatchOutcome.NoMatch, normalizedSubject, null, matches),
            1 => new MatchingDecision(
                MatchOutcome.Matched,
                normalizedSubject,
                matches.Single().ProjectId,
                matches),
            _ => new MatchingDecision(MatchOutcome.Conflict, normalizedSubject, null, matches),
        };
    }
}
