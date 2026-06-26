using MailAssistant.Domain.MailAccounts;
using MailAssistant.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MailAssistant.Infrastructure.Persistence.Configurations;

internal sealed class ProviderClassificationTargetConfiguration
    : IEntityTypeConfiguration<ProviderClassificationTarget>
{
    public void Configure(EntityTypeBuilder<ProviderClassificationTarget> builder)
    {
        builder.ToTable("provider_classification_targets");
        builder.HasKey(target => target.Id);
        builder.Property(target => target.Id).ValueGeneratedNever();
        builder.Property(target => target.ExternalTargetId).HasMaxLength(200).IsRequired();
        builder.Property(target => target.ExternalTargetName).HasMaxLength(200).IsRequired();
        builder.Property(target => target.CreatedAt).IsRequired();
        builder.Property(target => target.UpdatedAt).IsRequired();

        builder.HasIndex(target => new { target.MailAccountId, target.ProjectId }).IsUnique();

        builder.HasOne<MailAccount>()
            .WithMany()
            .HasForeignKey(target => target.MailAccountId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(target => target.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
