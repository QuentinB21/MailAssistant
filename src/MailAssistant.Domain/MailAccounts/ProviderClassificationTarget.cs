namespace MailAssistant.Domain.MailAccounts;

public sealed class ProviderClassificationTarget
{
    private ProviderClassificationTarget()
    {
    }

    private ProviderClassificationTarget(
        Guid id,
        Guid mailAccountId,
        Guid projectId,
        string externalTargetId,
        string externalTargetName,
        DateTimeOffset now)
    {
        Id = id;
        MailAccountId = mailAccountId;
        ProjectId = projectId;
        ExternalTargetId = Validate(externalTargetId, 200, nameof(externalTargetId));
        ExternalTargetName = Validate(externalTargetName, 200, nameof(externalTargetName));
        CreatedAt = now;
        UpdatedAt = now;
    }

    public Guid Id { get; private set; }

    public Guid MailAccountId { get; private set; }

    public Guid ProjectId { get; private set; }

    public string ExternalTargetId { get; private set; } = string.Empty;

    public string ExternalTargetName { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static ProviderClassificationTarget Create(
        Guid mailAccountId,
        Guid projectId,
        string externalTargetId,
        string externalTargetName,
        DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(mailAccountId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfEqual(projectId, Guid.Empty);

        return new ProviderClassificationTarget(
            Guid.NewGuid(),
            mailAccountId,
            projectId,
            externalTargetId,
            externalTargetName,
            now);
    }

    public void Update(
        string externalTargetId,
        string externalTargetName,
        DateTimeOffset now)
    {
        ExternalTargetId = Validate(externalTargetId, 200, nameof(externalTargetId));
        ExternalTargetName = Validate(externalTargetName, 200, nameof(externalTargetName));
        UpdatedAt = now;
    }

    private static string Validate(string value, int maximumLength, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        var trimmed = value.Trim();
        if (trimmed.Length > maximumLength)
        {
            throw new ArgumentException(
                $"Value cannot exceed {maximumLength} characters.",
                parameterName);
        }

        return trimmed;
    }
}
