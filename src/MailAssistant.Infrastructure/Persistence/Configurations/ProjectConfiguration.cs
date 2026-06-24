using MailAssistant.Domain.Organizations;
using MailAssistant.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MailAssistant.Infrastructure.Persistence.Configurations;

internal sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects");
        builder.HasKey(project => project.Id);
        builder.Property(project => project.Id).ValueGeneratedNever();
        builder.Property(project => project.Name).HasMaxLength(200).IsRequired();
        builder.Property(project => project.Description).HasMaxLength(2000);
        builder.Property(project => project.IsActive).IsRequired();
        builder.Property(project => project.ClassificationTargetName)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(project => project.CreatedAt).IsRequired();
        builder.Property(project => project.UpdatedAt).IsRequired();

        builder.HasIndex(project => new { project.OrganizationId, project.Name }).IsUnique();

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(project => project.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(project => project.Aliases)
            .WithOne()
            .HasForeignKey(alias => alias.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(project => project.Aliases)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
