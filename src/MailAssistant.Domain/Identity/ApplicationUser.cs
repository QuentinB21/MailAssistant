namespace MailAssistant.Domain.Identity;

public sealed class ApplicationUser
{
    private ApplicationUser()
    {
    }

    private ApplicationUser(
        Guid id,
        string subject,
        string? email,
        string displayName,
        DateTimeOffset createdAt)
    {
        Id = id;
        Subject = ValidateRequired(subject, 200, nameof(subject));
        Email = ValidateEmail(email);
        DisplayName = ValidateRequired(displayName, 200, nameof(displayName));
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public string Subject { get; private set; } = string.Empty;

    public string? Email { get; private set; }

    public string DisplayName { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static ApplicationUser Create(
        string subject,
        string? email,
        string displayName,
        DateTimeOffset now)
    {
        return new ApplicationUser(Guid.NewGuid(), subject, email, displayName, now);
    }

    public bool Synchronize(string? email, string displayName, DateTimeOffset now)
    {
        var validatedEmail = ValidateEmail(email);
        var validatedDisplayName = ValidateRequired(displayName, 200, nameof(displayName));
        if (string.Equals(Email, validatedEmail, StringComparison.OrdinalIgnoreCase)
            && string.Equals(DisplayName, validatedDisplayName, StringComparison.Ordinal))
        {
            return false;
        }

        Email = validatedEmail;
        DisplayName = validatedDisplayName;
        UpdatedAt = now;
        return true;
    }

    private static string ValidateRequired(string value, int maximumLength, string parameterName)
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

    private static string? ValidateEmail(string? email)
    {
        var trimmed = email?.Trim();
        if (trimmed?.Length > 320)
        {
            throw new ArgumentException("Email cannot exceed 320 characters.", nameof(email));
        }

        return string.IsNullOrWhiteSpace(trimmed)
            ? null
            : trimmed.ToLowerInvariant();
    }
}
