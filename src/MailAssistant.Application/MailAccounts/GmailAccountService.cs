using MailAssistant.Application.Abstractions;
using MailAssistant.Application.Common;
using MailAssistant.Application.Identity;
using MailAssistant.Application.Projects;
using MailAssistant.Domain.Identity;
using MailAssistant.Domain.MailAccounts;
using MailAssistant.Domain.Matching;

namespace MailAssistant.Application.MailAccounts;

public sealed class GmailAccountService(
    IMailAccountRepository mailAccounts,
    IOAuthCredentialRepository credentials,
    IProviderClassificationTargetRepository targets,
    IProjectRepository projects,
    IOrganizationSettingsRepository settings,
    IMembershipRepository memberships,
    OrganizationAccessService access,
    SubjectMatchingService matching,
    IGmailGateway gmail,
    IOAuthTokenProtector tokenProtector,
    IGmailAuthorizationStateProtector stateProtector,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
{
    private static readonly TimeSpan AuthorizationStateLifetime = TimeSpan.FromMinutes(10);

    public async Task<IReadOnlyCollection<GmailAccountResponse>> ListAsync(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        await access.RequireAsync(
            organizationId,
            OrganizationRole.Member,
            cancellationToken);
        var accounts = await mailAccounts.ListAsync(organizationId, cancellationToken);
        return accounts
            .Where(account => account.Provider == MailProvider.Gmail)
            .Select(Map)
            .ToArray();
    }

    public async Task<GmailAuthorizationResponse> CreateAuthorizationAsync(
        Guid organizationId,
        string redirectUri,
        CancellationToken cancellationToken)
    {
        var membership = await access.RequireAsync(
            organizationId,
            OrganizationRole.Admin,
            cancellationToken);
        EnsureConfigured();
        ValidateAbsoluteUri(redirectUri);

        var state = stateProtector.Protect(new GmailAuthorizationState(
            organizationId,
            membership.UserId,
            timeProvider.GetUtcNow().Add(AuthorizationStateLifetime)));
        return new GmailAuthorizationResponse(
            gmail.CreateAuthorizationUri(redirectUri, state).ToString());
    }

    public async Task<GmailCallbackResult> CompleteAuthorizationAsync(
        string code,
        string protectedState,
        string redirectUri,
        CancellationToken cancellationToken)
    {
        EnsureConfigured();
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(protectedState);
        ValidateAbsoluteUri(redirectUri);

        var state = stateProtector.Unprotect(protectedState);
        if (state.ExpiresAt < timeProvider.GetUtcNow())
        {
            throw new ArgumentException("The Gmail authorization request has expired.");
        }
        var membership = await memberships.GetAsync(
            state.OrganizationId,
            state.UserId,
            cancellationToken);
        if (membership is null || !membership.Role.HasAtLeast(OrganizationRole.Admin))
        {
            throw new AccessDeniedException(
                "The user who started the Gmail authorization can no longer manage this organization.");
        }

        var tokenResponse = await gmail.ExchangeCodeAsync(
            code,
            redirectUri,
            cancellationToken);
        if (string.IsNullOrWhiteSpace(tokenResponse.RefreshToken))
        {
            throw new ExternalIntegrationException(
                "Google did not return a refresh token. Revoke the existing consent and try again.");
        }

        var profile = await gmail.GetProfileAsync(
            tokenResponse.AccessToken,
            cancellationToken);
        var now = timeProvider.GetUtcNow();
        var account = await mailAccounts.GetByProviderAddressAsync(
            state.OrganizationId,
            MailProvider.Gmail,
            profile.EmailAddress,
            cancellationToken);

        if (account is null)
        {
            account = MailAccount.ConnectGmail(
                state.OrganizationId,
                state.UserId,
                profile.EmailAddress,
                now);
            await mailAccounts.AddAsync(account, cancellationToken);
        }
        else
        {
            account.RefreshConnection(state.UserId, now);
        }

        var protectedRefreshToken = tokenProtector.Protect(tokenResponse.RefreshToken);
        var credential = await credentials.GetAsync(account.Id, cancellationToken);
        if (credential is null)
        {
            credential = OAuthCredential.Create(
                account.Id,
                protectedRefreshToken,
                tokenResponse.GrantedScopes,
                now);
            await credentials.AddAsync(credential, cancellationToken);
        }
        else
        {
            credential.Update(
                protectedRefreshToken,
                tokenResponse.GrantedScopes,
                now);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new GmailCallbackResult(state.OrganizationId, account.EmailAddress);
    }

    public async Task<GmailAccountResponse> SetAutomaticClassificationAsync(
        Guid organizationId,
        Guid mailAccountId,
        bool enabled,
        CancellationToken cancellationToken)
    {
        await access.RequireAsync(
            organizationId,
            OrganizationRole.Admin,
            cancellationToken);
        var account = await GetAccountAsync(
            organizationId,
            mailAccountId,
            cancellationToken);
        account.SetAutomaticClassification(enabled, timeProvider.GetUtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(account);
    }

    public async Task DisconnectAsync(
        Guid organizationId,
        Guid mailAccountId,
        CancellationToken cancellationToken)
    {
        await access.RequireAsync(
            organizationId,
            OrganizationRole.Admin,
            cancellationToken);
        var account = await GetAccountAsync(
            organizationId,
            mailAccountId,
            cancellationToken);
        var credential = await credentials.GetAsync(account.Id, cancellationToken)
            ?? throw new KeyNotFoundException("OAuth credential not found.");
        var refreshToken = tokenProtector.Unprotect(credential.EncryptedRefreshToken);

        if (gmail.IsConfigured)
        {
            await gmail.RevokeAsync(refreshToken, cancellationToken);
        }

        credentials.Remove(credential);
        mailAccounts.Remove(account);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<GmailManualClassificationResponse> ClassifyMessageAsync(
        Guid organizationId,
        Guid mailAccountId,
        string messageId,
        CancellationToken cancellationToken)
    {
        await access.RequireAsync(
            organizationId,
            OrganizationRole.Admin,
            cancellationToken);
        EnsureConfigured();
        ArgumentException.ThrowIfNullOrWhiteSpace(messageId);

        var account = await GetAccountAsync(
            organizationId,
            mailAccountId,
            cancellationToken);
        var credential = await credentials.GetAsync(account.Id, cancellationToken)
            ?? throw new KeyNotFoundException("OAuth credential not found.");
        var accessToken = await gmail.RefreshAccessTokenAsync(
            tokenProtector.Unprotect(credential.EncryptedRefreshToken),
            cancellationToken);
        var subject = await gmail.GetMessageSubjectAsync(
            accessToken,
            messageId.Trim(),
            cancellationToken);
        var decision = await matching.TestSubjectAsync(
            organizationId,
            subject,
            cancellationToken);

        if (decision.Outcome != MatchOutcome.Matched
            || decision.SelectedProjectId is not Guid projectId)
        {
            return new GmailManualClassificationResponse(
                decision.Outcome,
                decision.NormalizedSubject,
                decision.SelectedProjectId,
                null,
                null,
                false,
                false);
        }

        var project = await projects.GetAsync(
            organizationId,
            projectId,
            cancellationToken)
            ?? throw new KeyNotFoundException("Project not found.");
        var target = await targets.GetAsync(account.Id, project.Id, cancellationToken);
        var labelId = target?.ExternalTargetId;
        if (target is null
            || !string.Equals(
                target.ExternalTargetName,
                project.ClassificationTargetName,
                StringComparison.Ordinal))
        {
            labelId = await gmail.FindLabelIdAsync(
                    accessToken,
                    project.ClassificationTargetName,
                    cancellationToken)
                ?? await gmail.CreateLabelAsync(
                    accessToken,
                    project.ClassificationTargetName,
                    cancellationToken);
            var now = timeProvider.GetUtcNow();
            if (target is null)
            {
                target = ProviderClassificationTarget.Create(
                    account.Id,
                    project.Id,
                    labelId,
                    project.ClassificationTargetName,
                    now);
                await targets.AddAsync(target, cancellationToken);
            }
            else
            {
                target.Update(labelId, project.ClassificationTargetName, now);
            }
        }

        var organizationSettings = await settings.GetAsync(
            organizationId,
            cancellationToken)
            ?? throw new KeyNotFoundException("Organization settings not found.");
        await gmail.ApplyLabelAsync(
            accessToken,
            messageId.Trim(),
            labelId ?? throw new InvalidOperationException(
                "The Gmail label could not be resolved."),
            organizationSettings.ArchiveGmailAfterClassification,
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new GmailManualClassificationResponse(
            decision.Outcome,
            decision.NormalizedSubject,
            project.Id,
            project.Name,
            project.ClassificationTargetName,
            true,
            organizationSettings.ArchiveGmailAfterClassification);
    }

    private async Task<MailAccount> GetAccountAsync(
        Guid organizationId,
        Guid mailAccountId,
        CancellationToken cancellationToken)
    {
        var account = await mailAccounts.GetAsync(
            organizationId,
            mailAccountId,
            cancellationToken);
        if (account is null || account.Provider != MailProvider.Gmail)
        {
            throw new KeyNotFoundException("Gmail account not found.");
        }

        return account;
    }

    private void EnsureConfigured()
    {
        if (!gmail.IsConfigured)
        {
            throw new IntegrationNotConfiguredException(
                "Gmail OAuth is not configured. Set the Google client ID and client secret.");
        }
    }

    private static void ValidateAbsoluteUri(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out _))
        {
            throw new ArgumentException("A valid absolute redirect URI is required.");
        }
    }

    private static GmailAccountResponse Map(MailAccount account)
    {
        return new GmailAccountResponse(
            account.Id,
            account.OrganizationId,
            account.EmailAddress,
            account.IsAutomaticClassificationEnabled,
            account.CreatedAt,
            account.UpdatedAt);
    }
}
