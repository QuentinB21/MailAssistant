namespace MailAssistant.Domain.Matching;

public enum MatchSource
{
    ProjectName = 1,
    Alias = 2,
}

public enum MatchOutcome
{
    NoMatch = 1,
    Matched = 2,
    Conflict = 3,
}

public enum MultipleMatchBehavior
{
    MarkAsConflict = 1,
}

public enum NoMatchBehavior
{
    Ignore = 1,
}
