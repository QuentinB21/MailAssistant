using MailAssistant.Domain.Matching;

namespace MailAssistant.Application.Projects;

public sealed record ProjectAliasResponse(
    Guid Id,
    string Value,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record ProjectResponse(
    Guid Id,
    Guid OrganizationId,
    string Name,
    string? Description,
    bool IsActive,
    string ClassificationTargetName,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyCollection<ProjectAliasResponse> Aliases);

public sealed record CreateProjectCommand(
    string Name,
    string ClassificationTargetName,
    string? Description);

public sealed record UpdateProjectCommand(
    string Name,
    string ClassificationTargetName,
    string? Description,
    bool IsActive);

public sealed record UpdateAliasCommand(string Value, bool IsActive);

public sealed record MatchingTestResponse(
    MatchOutcome Outcome,
    string NormalizedSubject,
    Guid? SelectedProjectId,
    IReadOnlyCollection<ProjectMatchResponse> Matches);

public sealed record ProjectMatchResponse(
    Guid ProjectId,
    string ProjectName,
    string MatchedValue,
    MatchSource Source);
