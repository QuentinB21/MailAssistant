using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MailAssistant.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationClassificationSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ArchiveGmailAfterClassification",
                table: "organization_settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArchiveGmailAfterClassification",
                table: "organization_settings");
        }
    }
}
