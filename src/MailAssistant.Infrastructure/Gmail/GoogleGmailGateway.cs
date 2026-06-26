using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using MailAssistant.Application.Abstractions;
using MailAssistant.Application.Common;
using Microsoft.Extensions.Configuration;

namespace MailAssistant.Infrastructure.Gmail;

internal sealed class GoogleGmailGateway(
    HttpClient httpClient,
    IConfiguration configuration) : IGmailGateway
{
    private const string GmailModifyScope =
        "https://www.googleapis.com/auth/gmail.modify";
    private static readonly string[] InboxLabel = ["INBOX"];
    private readonly string? _clientId = configuration["Gmail:ClientId"];
    private readonly string? _clientSecret = configuration["Gmail:ClientSecret"];

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_clientId)
        && !string.IsNullOrWhiteSpace(_clientSecret);

    public Uri CreateAuthorizationUri(string redirectUri, string protectedState)
    {
        EnsureConfigured();
        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = _clientId!,
            ["redirect_uri"] = redirectUri,
            ["response_type"] = "code",
            ["scope"] = GmailModifyScope,
            ["access_type"] = "offline",
            ["prompt"] = "consent",
            ["include_granted_scopes"] = "true",
            ["state"] = protectedState,
        };

        return new Uri(
            $"https://accounts.google.com/o/oauth2/v2/auth?{BuildQuery(parameters)}");
    }

    public async Task<GmailTokenResponse> ExchangeCodeAsync(
        string code,
        string redirectUri,
        CancellationToken cancellationToken)
    {
        EnsureConfigured();
        using var response = await httpClient.PostAsync(
            "https://oauth2.googleapis.com/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = _clientId!,
                ["client_secret"] = _clientSecret!,
                ["code"] = code,
                ["grant_type"] = "authorization_code",
                ["redirect_uri"] = redirectUri,
            }),
            cancellationToken);
        var payload = await ReadTokenResponseAsync(response, cancellationToken);
        return new GmailTokenResponse(
            payload.AccessToken!,
            payload.RefreshToken,
            payload.Scope ?? GmailModifyScope);
    }

    public async Task<string> RefreshAccessTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        EnsureConfigured();
        using var response = await httpClient.PostAsync(
            "https://oauth2.googleapis.com/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = _clientId!,
                ["client_secret"] = _clientSecret!,
                ["refresh_token"] = refreshToken,
                ["grant_type"] = "refresh_token",
            }),
            cancellationToken);
        return (await ReadTokenResponseAsync(response, cancellationToken)).AccessToken!;
    }

    public async Task<GmailProfile> GetProfileAsync(
        string accessToken,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(
            HttpMethod.Get,
            "https://gmail.googleapis.com/gmail/v1/users/me/profile",
            accessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "read the Gmail profile", cancellationToken);
        var profile = await response.Content.ReadFromJsonAsync<GmailProfilePayload>(
            cancellationToken);
        if (string.IsNullOrWhiteSpace(profile?.EmailAddress))
        {
            throw new ExternalIntegrationException(
                "Google returned an invalid Gmail profile.");
        }

        return new GmailProfile(profile.EmailAddress);
    }

    public async Task<string> GetMessageSubjectAsync(
        string accessToken,
        string messageId,
        CancellationToken cancellationToken)
    {
        var uri = "https://gmail.googleapis.com/gmail/v1/users/me/messages/"
            + Uri.EscapeDataString(messageId)
            + "?format=metadata&metadataHeaders=Subject";
        using var request = CreateAuthorizedRequest(HttpMethod.Get, uri, accessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "read the Gmail message metadata", cancellationToken);
        var message = await response.Content.ReadFromJsonAsync<GmailMessagePayload>(
            cancellationToken);
        return message?.Payload?.Headers?
            .SingleOrDefault(header =>
                string.Equals(header.Name, "Subject", StringComparison.OrdinalIgnoreCase))
            ?.Value
            ?? string.Empty;
    }

    public async Task<string?> FindLabelIdAsync(
        string accessToken,
        string labelName,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(
            HttpMethod.Get,
            "https://gmail.googleapis.com/gmail/v1/users/me/labels",
            accessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "list Gmail labels", cancellationToken);
        var labels = await response.Content.ReadFromJsonAsync<GmailLabelListPayload>(
            cancellationToken);
        return labels?.Labels?
            .SingleOrDefault(label =>
                string.Equals(label.Name, labelName, StringComparison.OrdinalIgnoreCase))
            ?.Id;
    }

    public async Task<string> CreateLabelAsync(
        string accessToken,
        string labelName,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(
            HttpMethod.Post,
            "https://gmail.googleapis.com/gmail/v1/users/me/labels",
            accessToken);
        request.Content = JsonContent.Create(new
        {
            name = labelName,
            labelListVisibility = "labelShow",
            messageListVisibility = "show",
        });
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "create the Gmail label", cancellationToken);
        var label = await response.Content.ReadFromJsonAsync<GmailLabelPayload>(
            cancellationToken);
        if (string.IsNullOrWhiteSpace(label?.Id))
        {
            throw new ExternalIntegrationException(
                "Google returned an invalid Gmail label.");
        }

        return label.Id;
    }

    public async Task ApplyLabelAsync(
        string accessToken,
        string messageId,
        string labelId,
        bool archive,
        CancellationToken cancellationToken)
    {
        var uri = "https://gmail.googleapis.com/gmail/v1/users/me/messages/"
            + Uri.EscapeDataString(messageId)
            + "/modify";
        using var request = CreateAuthorizedRequest(HttpMethod.Post, uri, accessToken);
        request.Content = JsonContent.Create(new
        {
            addLabelIds = new[] { labelId },
            removeLabelIds = archive ? InboxLabel : Array.Empty<string>(),
        });
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "apply the Gmail label", cancellationToken);
    }

    public async Task RevokeAsync(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsync(
            "https://oauth2.googleapis.com/revoke",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["token"] = refreshToken,
            }),
            cancellationToken);
        if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.BadRequest)
        {
            return;
        }

        throw new ExternalIntegrationException(
            $"Google could not revoke the Gmail authorization ({(int)response.StatusCode}).");
    }

    private static HttpRequestMessage CreateAuthorizedRequest(
        HttpMethod method,
        string uri,
        string accessToken)
    {
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private static async Task<GoogleTokenPayload> ReadTokenResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw new ExternalIntegrationException(
                $"Google rejected the OAuth token request ({(int)response.StatusCode}).");
        }

        var payload = await response.Content.ReadFromJsonAsync<GoogleTokenPayload>(
            cancellationToken);
        if (string.IsNullOrWhiteSpace(payload?.AccessToken))
        {
            throw new ExternalIntegrationException(
                "Google returned an invalid OAuth token response.");
        }

        return payload;
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        string operation,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        await response.Content.LoadIntoBufferAsync(cancellationToken);
        throw new ExternalIntegrationException(
            $"Google could not {operation} ({(int)response.StatusCode}).");
    }

    private void EnsureConfigured()
    {
        if (!IsConfigured)
        {
            throw new IntegrationNotConfiguredException(
                "Gmail OAuth is not configured.");
        }
    }

    private static string BuildQuery(IReadOnlyDictionary<string, string> parameters)
    {
        return string.Join(
            "&",
            parameters.Select(pair =>
                $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}"));
    }

    private sealed record GoogleTokenPayload(
        [property: JsonPropertyName("access_token")] string? AccessToken,
        [property: JsonPropertyName("refresh_token")] string? RefreshToken,
        [property: JsonPropertyName("scope")] string? Scope);

    private sealed record GmailProfilePayload(
        [property: JsonPropertyName("emailAddress")] string EmailAddress);

    private sealed record GmailMessagePayload(
        [property: JsonPropertyName("payload")] GmailMessagePartPayload? Payload);

    private sealed record GmailMessagePartPayload(
        [property: JsonPropertyName("headers")] GmailHeaderPayload[]? Headers);

    private sealed record GmailHeaderPayload(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("value")] string Value);

    private sealed record GmailLabelListPayload(
        [property: JsonPropertyName("labels")] GmailLabelPayload[]? Labels);

    private sealed record GmailLabelPayload(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("name")] string Name);
}
