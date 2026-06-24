using MailAssistant.Domain.Matching;

namespace MailAssistant.Domain.Organizations;

public sealed class OrganizationSettings
{
    private OrganizationSettings()
    {
    }

    private OrganizationSettings(Guid organizationId)
    {
        OrganizationId = organizationId;
    }

    public Guid OrganizationId { get; private set; }

    public MultipleMatchBehavior MultipleMatchBehavior { get; private set; } =
        MultipleMatchBehavior.MarkAsConflict;

    public NoMatchBehavior NoMatchBehavior { get; private set; } = NoMatchBehavior.Ignore;

    public bool ArchiveGmailAfterClassification { get; private set; }

    public static OrganizationSettings Create(Guid organizationId)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(organizationId, Guid.Empty);
        return new OrganizationSettings(organizationId);
    }

    public void Update(
        MultipleMatchBehavior multipleMatchBehavior,
        NoMatchBehavior noMatchBehavior,
        bool archiveGmailAfterClassification)
    {
        MultipleMatchBehavior = multipleMatchBehavior;
        NoMatchBehavior = noMatchBehavior;
        ArchiveGmailAfterClassification = archiveGmailAfterClassification;
    }
}
