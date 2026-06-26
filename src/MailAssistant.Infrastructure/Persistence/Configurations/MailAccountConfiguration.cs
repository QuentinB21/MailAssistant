using MailAssistant.Domain.Identity;
using MailAssistant.Domain.MailAccounts;
using MailAssistant.Domain.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MailAssistant.Infrastructure.Persistence.Configurations;

internal sealed class MailAccountConfiguration : IEntityTypeConfiguration<MailAccount>
{
    public void Configure(EntityTypeBuilder<MailAccount> builder)
    {
        builder.ToTable("mail_accounts");
        builder.HasKey(account => account.Id);
        builder.Property(account => account.Id).ValueGeneratedNever();
        builder.Property(account => account.Provider).HasConversion<string>().HasMaxLength(20);
        builder.Property(account => account.EmailAddress).HasMaxLength(320).IsRequired();
        builder.Property(account => account.IsAutomaticClassificationEnabled).IsRequired();
        builder.Property(account => account.CreatedAt).IsRequired();
        builder.Property(account => account.UpdatedAt).IsRequired();

        builder.HasIndex(account => new
        {
            account.OrganizationId,
            account.Provider,
            account.EmailAddress,
        }).IsUnique();

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(account => account.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(account => account.ConnectedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
