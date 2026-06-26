using MailAssistant.Application.Abstractions;
using Microsoft.AspNetCore.DataProtection;

namespace MailAssistant.Infrastructure.Security;

internal sealed class DataProtectionOAuthTokenProtector(
    IDataProtectionProvider dataProtectionProvider) : IOAuthTokenProtector
{
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector(
        "MailAssistant.OAuth.RefreshTokens.v1");

    public string Protect(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        return _protector.Protect(token);
    }

    public string Unprotect(string protectedToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(protectedToken);
        return _protector.Unprotect(protectedToken);
    }
}
