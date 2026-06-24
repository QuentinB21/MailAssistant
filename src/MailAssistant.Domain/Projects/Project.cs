namespace MailAssistant.Domain.Projects;

public sealed class Project
{
    private readonly List<ProjectAlias> _aliases = [];

    private Project()
    {
    }

    private Project(
        Guid id,
        Guid organizationId,
        string name,
        string classificationTargetName,
        string? description,
        DateTimeOffset createdAt)
    {
        Id = id;
        OrganizationId = organizationId;
        Name = ValidateRequiredText(name, 200, nameof(name));
        ClassificationTargetName = ValidateRequiredText(
            classificationTargetName,
            200,
            nameof(classificationTargetName));
        Description = ValidateDescription(description);
        IsActive = true;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public Guid OrganizationId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public string ClassificationTargetName { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyCollection<ProjectAlias> Aliases => _aliases.AsReadOnly();

    public static Project Create(
        Guid organizationId,
        string name,
        string classificationTargetName,
        string? description,
        DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(organizationId, Guid.Empty);

        return new Project(
            Guid.NewGuid(),
            organizationId,
            name,
            classificationTargetName,
            description,
            now);
    }

    public void Update(
        string name,
        string classificationTargetName,
        string? description,
        bool isActive,
        DateTimeOffset now)
    {
        Name = ValidateRequiredText(name, 200, nameof(name));
        ClassificationTargetName = ValidateRequiredText(
            classificationTargetName,
            200,
            nameof(classificationTargetName));
        Description = ValidateDescription(description);
        IsActive = isActive;
        UpdatedAt = now;
    }

    public ProjectAlias AddAlias(string value, DateTimeOffset now)
    {
        var normalizedValue = ProjectAlias.ValidateValue(value);
        if (_aliases.Any(alias =>
            string.Equals(alias.Value, normalizedValue, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("This alias already exists on the project.");
        }

        var alias = ProjectAlias.Create(Id, normalizedValue, now);
        _aliases.Add(alias);
        UpdatedAt = now;
        return alias;
    }

    public void UpdateAlias(Guid aliasId, string value, bool isActive, DateTimeOffset now)
    {
        var alias = GetAlias(aliasId);
        var normalizedValue = ProjectAlias.ValidateValue(value);

        if (_aliases.Any(existingAlias =>
            existingAlias.Id != aliasId
            && string.Equals(existingAlias.Value, normalizedValue, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("This alias already exists on the project.");
        }

        alias.Update(normalizedValue, isActive, now);
        UpdatedAt = now;
    }

    public void RemoveAlias(Guid aliasId, DateTimeOffset now)
    {
        var alias = GetAlias(aliasId);
        _aliases.Remove(alias);
        UpdatedAt = now;
    }

    private ProjectAlias GetAlias(Guid aliasId)
    {
        return _aliases.SingleOrDefault(alias => alias.Id == aliasId)
            ?? throw new KeyNotFoundException("Alias not found.");
    }

    private static string ValidateRequiredText(string value, int maximumLength, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);

        var trimmedValue = value.Trim();
        if (trimmedValue.Length > maximumLength)
        {
            throw new ArgumentException(
                $"Value cannot exceed {maximumLength} characters.",
                parameterName);
        }

        return trimmedValue;
    }

    private static string? ValidateDescription(string? description)
    {
        var trimmedDescription = description?.Trim();
        if (trimmedDescription?.Length > 2000)
        {
            throw new ArgumentException(
                "Description cannot exceed 2000 characters.",
                nameof(description));
        }

        return string.IsNullOrEmpty(trimmedDescription) ? null : trimmedDescription;
    }
}
