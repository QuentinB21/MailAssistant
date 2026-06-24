namespace MailAssistant.Domain.Projects;

public sealed class ProjectAlias
{
    private ProjectAlias()
    {
    }

    private ProjectAlias(Guid id, Guid projectId, string value, DateTimeOffset createdAt)
    {
        Id = id;
        ProjectId = projectId;
        Value = value;
        IsActive = true;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public Guid ProjectId { get; private set; }

    public string Value { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    internal static ProjectAlias Create(Guid projectId, string value, DateTimeOffset now)
    {
        return new ProjectAlias(Guid.NewGuid(), projectId, ValidateValue(value), now);
    }

    internal void Update(string value, bool isActive, DateTimeOffset now)
    {
        Value = ValidateValue(value);
        IsActive = isActive;
        UpdatedAt = now;
    }

    internal static string ValidateValue(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var trimmedValue = value.Trim();
        if (trimmedValue.Length > 200)
        {
            throw new ArgumentException("Alias cannot exceed 200 characters.", nameof(value));
        }

        return trimmedValue;
    }
}
