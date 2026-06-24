namespace MailAssistant.Domain.Organizations;

public sealed class Organization
{
    private Organization()
    {
    }

    private Organization(Guid id, string name, DateTimeOffset createdAt)
    {
        Id = id;
        Name = name;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static Organization Create(string name, DateTimeOffset now)
    {
        return new Organization(Guid.NewGuid(), ValidateName(name), now);
    }

    public void Rename(string name, DateTimeOffset now)
    {
        Name = ValidateName(name);
        UpdatedAt = now;
    }

    private static string ValidateName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var trimmedName = name.Trim();
        if (trimmedName.Length > 200)
        {
            throw new ArgumentException("Organization name cannot exceed 200 characters.", nameof(name));
        }

        return trimmedName;
    }
}
