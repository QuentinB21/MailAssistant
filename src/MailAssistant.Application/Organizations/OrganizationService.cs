using MailAssistant.Application.Abstractions;
using MailAssistant.Domain.Organizations;

namespace MailAssistant.Application.Organizations;

public sealed class OrganizationService(
    IOrganizationRepository organizations,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
{
    public async Task<IReadOnlyCollection<OrganizationResponse>> ListAsync(
        CancellationToken cancellationToken)
    {
        var results = await organizations.ListAsync(cancellationToken);
        return results.Select(Map).ToArray();
    }

    public async Task<OrganizationResponse> GetAsync(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var organization = await organizations.GetAsync(organizationId, cancellationToken)
            ?? throw new KeyNotFoundException("Organization not found.");

        return Map(organization);
    }

    public async Task<OrganizationResponse> CreateAsync(
        string name,
        CancellationToken cancellationToken)
    {
        var organization = Organization.Create(name, timeProvider.GetUtcNow());

        await organizations.AddAsync(organization, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(organization);
    }

    private static OrganizationResponse Map(Organization organization)
    {
        return new OrganizationResponse(
            organization.Id,
            organization.Name,
            organization.CreatedAt,
            organization.UpdatedAt);
    }
}
