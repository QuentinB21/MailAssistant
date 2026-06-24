using MailAssistant.Application.Abstractions;
using MailAssistant.Application.Identity;
using MailAssistant.Domain.Identity;
using MailAssistant.Domain.Organizations;

namespace MailAssistant.Application.Organizations;

public sealed class OrganizationService(
    IOrganizationRepository organizations,
    IMembershipRepository memberships,
    CurrentUserService currentUserService,
    OrganizationAccessService access,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
{
    public async Task<IReadOnlyCollection<OrganizationResponse>> ListAsync(
        CancellationToken cancellationToken)
    {
        var user = await currentUserService.GetOrCreateAsync(cancellationToken);
        var results = await organizations.ListForUserAsync(user.Id, cancellationToken);
        var responses = new List<OrganizationResponse>(results.Count);

        foreach (var organization in results)
        {
            var membership = await memberships.GetAsync(
                organization.Id,
                user.Id,
                cancellationToken);
            responses.Add(Map(
                organization,
                membership?.Role
                    ?? throw new InvalidOperationException("Membership is missing.")));
        }

        return responses;
    }

    public async Task<OrganizationResponse> GetAsync(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var membership = await access.RequireAsync(
            organizationId,
            OrganizationRole.Member,
            cancellationToken);
        var organization = await organizations.GetAsync(organizationId, cancellationToken)
            ?? throw new KeyNotFoundException("Organization not found.");

        return Map(organization, membership.Role);
    }

    public async Task<OrganizationResponse> CreateAsync(
        string name,
        CancellationToken cancellationToken)
    {
        var user = await currentUserService.GetOrCreateAsync(cancellationToken);
        var organization = Organization.Create(name, timeProvider.GetUtcNow());
        var membership = OrganizationMembership.Create(
            organization.Id,
            user.Id,
            OrganizationRole.Owner,
            timeProvider.GetUtcNow());

        await organizations.AddAsync(organization, cancellationToken);
        await memberships.AddAsync(membership, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(organization, membership.Role);
    }

    private static OrganizationResponse Map(
        Organization organization,
        OrganizationRole role)
    {
        return new OrganizationResponse(
            organization.Id,
            organization.Name,
            role,
            organization.CreatedAt,
            organization.UpdatedAt);
    }
}
