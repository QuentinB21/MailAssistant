using MailAssistant.Domain.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MailAssistant.Infrastructure.Persistence.Configurations;

internal sealed class OrganizationSettingsConfiguration
    : IEntityTypeConfiguration<OrganizationSettings>
{
    public void Configure(EntityTypeBuilder<OrganizationSettings> builder)
    {
        builder.ToTable("organization_settings");
        builder.HasKey(settings => settings.OrganizationId);
        builder.Property(settings => settings.MultipleMatchBehavior)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(settings => settings.NoMatchBehavior)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(settings => settings.ArchiveGmailAfterClassification)
            .IsRequired();

        builder.HasOne<Organization>()
            .WithOne()
            .HasForeignKey<OrganizationSettings>(settings => settings.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
