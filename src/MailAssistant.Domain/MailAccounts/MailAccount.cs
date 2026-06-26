namespace MailAssistant.Domain.MailAccounts;

public sealed class MailAccount
{
    private MailAccount()
    {
    }

    private MailAccount(
        Guid id,
        Guid organizationId,
        Guid connectedByUserId,
        MailProvider provider,
        string emailAddress,
        DateTimeOffset now)
    {
        Id = id;
        OrganizationId = organizationId;
        ConnectedByUserId = connectedByUserId;
        Provider = provider;
        EmailAddress = NormalizeEmail(emailAddress);
        IsAutomaticClassificationEnabled = false;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public Guid Id { get; private set; }

    public Guid OrganizationId { get; private set; }

    public Guid ConnectedByUserId { get; private set; }

    public MailProvider Provider { get; private set; }

    public string EmailAddress { get; private set; } = string.Empty;

    public bool IsAutomaticClassificationEnabled { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static MailAccount ConnectGmail(
        Guid organizationId,
        Guid connectedByUserId,
        string emailAddress,
        DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(organizationId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfEqual(connectedByUserId, Guid.Empty);

        return new MailAccount(
            Guid.NewGuid(),
            organizationId,
            connectedByUserId,
            MailProvider.Gmail,
            emailAddress,
            now);
    }

    public void RefreshConnection(Guid connectedByUserId, DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(connectedByUserId, Guid.Empty);
        ConnectedByUserId = connectedByUserId;
        UpdatedAt = now;
    }

    public void SetAutomaticClassification(bool enabled, DateTimeOffset now)
    {
        IsAutomaticClassificationEnabled = enabled;
        UpdatedAt = now;
    }

    private static string NormalizeEmail(string emailAddress)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(emailAddress);
        var normalized = emailAddress.Trim().ToLowerInvariant();
        if (normalized.Length > 320 || !normalized.Contains('@', StringComparison.Ordinal))
        {
            throw new ArgumentException("A valid email address is required.", nameof(emailAddress));
        }

        return normalized;
    }
}
