using MailAssistant.Application.Abstractions;
using MailAssistant.Application.Identity;
using MailAssistant.Domain.Identity;
using MailAssistant.Domain.Matching;

namespace MailAssistant.Application.Projects;

public sealed class SubjectMatchingService(
    IProjectRepository projects,
    OrganizationAccessService access,
    ISubjectNormalizer subjectNormalizer,
    IMatchingStrategy matchingStrategy,
    IConflictResolutionPolicy conflictResolutionPolicy)
{
    public async Task<MatchingTestResponse> TestSubjectAsync(
        Guid organizationId,
        string subject,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(subject);
        await access.RequireAsync(
            organizationId,
            OrganizationRole.Member,
            cancellationToken);

        var organizationProjects = await projects.ListAsync(
            organizationId,
            cancellationToken);
        var definitions = organizationProjects
            .Select(project => new ProjectMatchDefinition(
                project.Id,
                project.Name,
                project.IsActive,
                project.Aliases
                    .Select(alias => new ProjectAliasMatchDefinition(alias.Value, alias.IsActive))
                    .ToArray()))
            .ToArray();

        var normalizedSubject = subjectNormalizer.Normalize(subject);
        var matches = matchingStrategy.FindMatches(normalizedSubject, definitions);
        var decision = conflictResolutionPolicy.Resolve(normalizedSubject, matches);

        return new MatchingTestResponse(
            decision.Outcome,
            decision.NormalizedSubject,
            decision.SelectedProjectId,
            decision.Matches
                .Select(match => new ProjectMatchResponse(
                    match.ProjectId,
                    match.ProjectName,
                    match.MatchedValue,
                    match.Source))
                .ToArray());
    }
}
