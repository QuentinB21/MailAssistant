namespace MailAssistant.Domain.Matching;

public sealed class ContainsSubjectMatchingStrategy(ISubjectNormalizer normalizer) : IMatchingStrategy
{
    public IReadOnlyCollection<ProjectMatch> FindMatches(
        string normalizedSubject,
        IReadOnlyCollection<ProjectMatchDefinition> projects)
    {
        ArgumentNullException.ThrowIfNull(normalizedSubject);
        ArgumentNullException.ThrowIfNull(projects);

        if (normalizedSubject.Length == 0)
        {
            return [];
        }

        var paddedSubject = $" {normalizedSubject} ";
        var matches = new List<ProjectMatch>();

        foreach (var project in projects.Where(project => project.IsActive))
        {
            var projectName = normalizer.Normalize(project.ProjectName);
            if (ContainsWholeTerm(paddedSubject, projectName))
            {
                matches.Add(new ProjectMatch(
                    project.ProjectId,
                    project.ProjectName,
                    project.ProjectName,
                    MatchSource.ProjectName));
            }

            foreach (var alias in project.Aliases.Where(alias => alias.IsActive))
            {
                var normalizedAlias = normalizer.Normalize(alias.Value);
                if (ContainsWholeTerm(paddedSubject, normalizedAlias))
                {
                    matches.Add(new ProjectMatch(
                        project.ProjectId,
                        project.ProjectName,
                        alias.Value,
                        MatchSource.Alias));
                }
            }
        }

        return matches
            .GroupBy(match => match.ProjectId)
            .Select(group => group
                .OrderByDescending(match => normalizer.Normalize(match.MatchedValue).Length)
                .ThenBy(match => match.Source)
                .First())
            .OrderBy(match => match.ProjectName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static bool ContainsWholeTerm(string paddedSubject, string normalizedTerm)
    {
        return normalizedTerm.Length > 0
            && paddedSubject.Contains($" {normalizedTerm} ", StringComparison.Ordinal);
    }
}
