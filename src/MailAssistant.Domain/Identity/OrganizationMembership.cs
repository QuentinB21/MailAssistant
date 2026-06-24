namespace MailAssistant.Domain.Identity;

public sealed class OrganizationMembership
{
    private OrganizationMembership()
    {
    }

    private OrganizationMembership(
        Guid organizationId,
        Guid userId,
        OrganizationRole role,
        DateTimeOffset createdAt)
    {
        OrganizationId = organizationId;
        UserId = userId;
        Role = role;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid OrganizationId { get; private set; }

    public Guid UserId { get; private set; }

    public OrganizationRole Role { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static OrganizationMembership Create(
        Guid organizationId,
        Guid userId,
        OrganizationRole role,
        DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(organizationId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfEqual(userId, Guid.Empty);
        return new OrganizationMembership(organizationId, userId, role, now);
    }

    public void ChangeRole(OrganizationRole role, DateTimeOffset now)
    {
        Role = role;
        UpdatedAt = now;
    }
}
