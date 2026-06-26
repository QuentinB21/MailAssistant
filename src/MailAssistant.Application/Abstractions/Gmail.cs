namespace MailAssistant.Application.Abstractions;

public sealed record GmailAuthorizationState(
    Guid OrganizationId,
    Guid UserId,
    DateTimeOffset ExpiresAt);

public sealed record GmailTokenResponse(
    string AccessToken,
    string? RefreshToken,
    string GrantedScopes);

public sealed record GmailProfile(string EmailAddress);

public interface IOAuthTokenProtector
{
    string Protect(string token);

    string Unprotect(string protectedToken);
}

public interface IGmailAuthorizationStateProtector
{
    string Protect(GmailAuthorizationState state);

    GmailAuthorizationState Unprotect(string protectedState);
}

public interface IGmailGateway
{
    bool IsConfigured { get; }

    Uri CreateAuthorizationUri(string redirectUri, string protectedState);

    Task<GmailTokenResponse> ExchangeCodeAsync(
        string code,
        string redirectUri,
        CancellationToken cancellationToken);

    Task<string> RefreshAccessTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken);

    Task<GmailProfile> GetProfileAsync(
        string accessToken,
        CancellationToken cancellationToken);

    Task<string> GetMessageSubjectAsync(
        string accessToken,
        string messageId,
        CancellationToken cancellationToken);

    Task<string?> FindLabelIdAsync(
        string accessToken,
        string labelName,
        CancellationToken cancellationToken);

    Task<string> CreateLabelAsync(
        string accessToken,
        string labelName,
        CancellationToken cancellationToken);

    Task ApplyLabelAsync(
        string accessToken,
        string messageId,
        string labelId,
        bool archive,
        CancellationToken cancellationToken);

    Task RevokeAsync(string refreshToken, CancellationToken cancellationToken);
}
