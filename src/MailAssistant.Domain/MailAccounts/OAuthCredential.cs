namespace MailAssistant.Domain.MailAccounts;

public sealed class OAuthCredential
{
    private OAuthCredential()
    {
    }

    private OAuthCredential(
        Guid mailAccountId,
        string encryptedRefreshToken,
        string grantedScopes,
        DateTimeOffset now)
    {
        MailAccountId = mailAccountId;
        Update(encryptedRefreshToken, grantedScopes, now);
        CreatedAt = now;
    }

    public Guid MailAccountId { get; private set; }

    public string EncryptedRefreshToken { get; private set; } = string.Empty;

    public string GrantedScopes { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static OAuthCredential Create(
        Guid mailAccountId,
        string encryptedRefreshToken,
        string grantedScopes,
        DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(mailAccountId, Guid.Empty);
        return new OAuthCredential(mailAccountId, encryptedRefreshToken, grantedScopes, now);
    }

    public void Update(
        string encryptedRefreshToken,
        string grantedScopes,
        DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(encryptedRefreshToken);
        ArgumentException.ThrowIfNullOrWhiteSpace(grantedScopes);

        EncryptedRefreshToken = encryptedRefreshToken;
        GrantedScopes = grantedScopes.Trim();
        UpdatedAt = now;
    }
}
