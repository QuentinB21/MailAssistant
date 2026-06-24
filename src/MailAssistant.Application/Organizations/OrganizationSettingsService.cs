using MailAssistant.Application.Abstractions;
using MailAssistant.Application.Identity;
using MailAssistant.Domain.Identity;
using MailAssistant.Domain.Organizations;

namespace MailAssistant.Application.Organizations;

public sealed class OrganizationSettingsService(
    IOrganizationSettingsRepository settings,
    OrganizationAccessService access,
    IUnitOfWork unitOfWork)
{
    public async Task<OrganizationSettingsResponse> GetAsync(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        await access.RequireAsync(
            organizationId,
            OrganizationRole.Member,
            cancellationToken);

        return Map(await GetSettingsAsync(organizationId, cancellationToken));
    }

    public async Task<OrganizationSettingsResponse> UpdateAsync(
        Guid organizationId,
        UpdateOrganizationSettingsCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        await access.RequireAsync(
            organizationId,
            OrganizationRole.Admin,
            cancellationToken);

        var organizationSettings = await GetSettingsAsync(
            organizationId,
            cancellationToken);
        organizationSettings.Update(
            command.MultipleMatchBehavior,
            command.NoMatchBehavior,
            command.ArchiveGmailAfterClassification);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(organizationSettings);
    }

    private async Task<OrganizationSettings> GetSettingsAsync(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        return await settings.GetAsync(organizationId, cancellationToken)
            ?? throw new KeyNotFoundException("Organization settings not found.");
    }

    private static OrganizationSettingsResponse Map(
        OrganizationSettings organizationSettings)
    {
        return new OrganizationSettingsResponse(
            organizationSettings.OrganizationId,
            organizationSettings.MultipleMatchBehavior,
            organizationSettings.NoMatchBehavior,
            organizationSettings.ArchiveGmailAfterClassification);
    }
}
