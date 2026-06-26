using MailAssistant.Domain.MailAccounts;
using Xunit;

namespace MailAssistant.UnitTests.MailAccounts;

public sealed class MailAccountTests
{
    [Fact]
    public void ConnectGmailNormalizesAddressAndDisablesAutomaticClassification()
    {
        var now = DateTimeOffset.UtcNow;

        var account = MailAccount.ConnectGmail(
            Guid.NewGuid(),
            Guid.NewGuid(),
            " Owner@Example.Test ",
            now);

        Assert.Equal(MailProvider.Gmail, account.Provider);
        Assert.Equal("owner@example.test", account.EmailAddress);
        Assert.False(account.IsAutomaticClassificationEnabled);
        Assert.Equal(now, account.CreatedAt);
    }

    [Fact]
    public void SetAutomaticClassificationUpdatesState()
    {
        var account = MailAccount.ConnectGmail(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "owner@example.test",
            DateTimeOffset.UtcNow);
        var updatedAt = DateTimeOffset.UtcNow.AddMinutes(1);

        account.SetAutomaticClassification(true, updatedAt);

        Assert.True(account.IsAutomaticClassificationEnabled);
        Assert.Equal(updatedAt, account.UpdatedAt);
    }

    [Fact]
    public void OAuthCredentialUpdateReplacesProtectedTokenWithoutExposingPlaintext()
    {
        var credential = OAuthCredential.Create(
            Guid.NewGuid(),
            "protected-token-1",
            "gmail.modify",
            DateTimeOffset.UtcNow);
        var updatedAt = DateTimeOffset.UtcNow.AddMinutes(1);

        credential.Update("protected-token-2", "gmail.modify", updatedAt);

        Assert.Equal("protected-token-2", credential.EncryptedRefreshToken);
        Assert.Equal(updatedAt, credential.UpdatedAt);
    }
}
