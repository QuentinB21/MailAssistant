using MailAssistant.Domain.Identity;

namespace MailAssistant.Application.Identity;

public sealed record MembershipResponse(
    Guid UserId,
    string? Email,
    string DisplayName,
    OrganizationRole Role,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
