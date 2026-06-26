using MailAssistant.Domain.MailAccounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MailAssistant.Infrastructure.Persistence.Configurations;

internal sealed class OAuthCredentialConfiguration : IEntityTypeConfiguration<OAuthCredential>
{
    public void Configure(EntityTypeBuilder<OAuthCredential> builder)
    {
        builder.ToTable("oauth_credentials");
        builder.HasKey(credential => credential.MailAccountId);
        builder.Property(credential => credential.MailAccountId).ValueGeneratedNever();
        builder.Property(credential => credential.EncryptedRefreshToken).IsRequired();
        builder.Property(credential => credential.GrantedScopes).HasMaxLength(2000).IsRequired();
        builder.Property(credential => credential.CreatedAt).IsRequired();
        builder.Property(credential => credential.UpdatedAt).IsRequired();

        builder.HasOne<MailAccount>()
            .WithOne()
            .HasForeignKey<OAuthCredential>(credential => credential.MailAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
