using MailAssistant.Domain.Identity;

namespace MailAssistant.Application.Organizations;

public sealed record OrganizationResponse(
    Guid Id,
    string Name,
    OrganizationRole Role,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
