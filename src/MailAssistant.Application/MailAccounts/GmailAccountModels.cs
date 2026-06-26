using MailAssistant.Domain.Matching;

namespace MailAssistant.Application.MailAccounts;

public sealed record GmailAccountResponse(
    Guid Id,
    Guid OrganizationId,
    string EmailAddress,
    bool IsAutomaticClassificationEnabled,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record GmailAuthorizationResponse(string AuthorizationUrl);

public sealed record GmailCallbackResult(Guid OrganizationId, string EmailAddress);

public sealed record GmailManualClassificationResponse(
    MatchOutcome Outcome,
    string NormalizedSubject,
    Guid? ProjectId,
    string? ProjectName,
    string? LabelName,
    bool LabelApplied,
    bool Archived);
