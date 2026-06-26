using System.Security.Cryptography;
using System.Text.Json;
using MailAssistant.Application.Abstractions;
using Microsoft.AspNetCore.DataProtection;

namespace MailAssistant.Infrastructure.Security;

internal sealed class DataProtectionGmailAuthorizationStateProtector(
    IDataProtectionProvider dataProtectionProvider) : IGmailAuthorizationStateProtector
{
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector(
        "MailAssistant.Gmail.AuthorizationState.v1");

    public string Protect(GmailAuthorizationState state)
    {
        return _protector.Protect(JsonSerializer.Serialize(state));
    }

    public GmailAuthorizationState Unprotect(string protectedState)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(protectedState);
        try
        {
            return JsonSerializer.Deserialize<GmailAuthorizationState>(
                    _protector.Unprotect(protectedState))
                ?? throw new ArgumentException("The Gmail authorization state is invalid.");
        }
        catch (CryptographicException exception)
        {
            throw new ArgumentException(
                "The Gmail authorization state is invalid.",
                nameof(protectedState),
                exception);
        }
        catch (JsonException exception)
        {
            throw new ArgumentException(
                "The Gmail authorization state is invalid.",
                nameof(protectedState),
                exception);
        }
    }
}
