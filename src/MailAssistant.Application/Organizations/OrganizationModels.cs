namespace MailAssistant.Application.Organizations;

public sealed record OrganizationResponse(
    Guid Id,
    string Name,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
