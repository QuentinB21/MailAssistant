using MailAssistant.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MailAssistant.Infrastructure.Persistence.Configurations;

internal sealed class ProjectAliasConfiguration : IEntityTypeConfiguration<ProjectAlias>
{
    public void Configure(EntityTypeBuilder<ProjectAlias> builder)
    {
        builder.ToTable("project_aliases");
        builder.HasKey(alias => alias.Id);
        builder.Property(alias => alias.Id).ValueGeneratedNever();
        builder.Property(alias => alias.Value).HasMaxLength(200).IsRequired();
        builder.Property(alias => alias.IsActive).IsRequired();
        builder.Property(alias => alias.CreatedAt).IsRequired();
        builder.Property(alias => alias.UpdatedAt).IsRequired();
        builder.HasIndex(alias => new { alias.ProjectId, alias.Value }).IsUnique();
    }
}
