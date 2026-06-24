using MailAssistant.Domain.Matching;

namespace MailAssistant.Application.Organizations;

public sealed record OrganizationSettingsResponse(
    Guid OrganizationId,
    MultipleMatchBehavior MultipleMatchBehavior,
    NoMatchBehavior NoMatchBehavior,
    bool ArchiveGmailAfterClassification);

public sealed record UpdateOrganizationSettingsCommand(
    MultipleMatchBehavior MultipleMatchBehavior,
    NoMatchBehavior NoMatchBehavior,
    bool ArchiveGmailAfterClassification);
