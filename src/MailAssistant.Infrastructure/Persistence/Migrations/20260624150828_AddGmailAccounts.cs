using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MailAssistant.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGmailAccounts : Migration
    {
        private static readonly string[] MailAccountUniqueIndexColumns =
            ["OrganizationId", "Provider", "EmailAddress"];

        private static readonly string[] ClassificationTargetUniqueIndexColumns =
            ["MailAccountId", "ProjectId"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mail_accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConnectedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EmailAddress = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    IsAutomaticClassificationEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mail_accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_mail_accounts_organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_mail_accounts_users_ConnectedByUserId",
                        column: x => x.ConnectedByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "oauth_credentials",
                columns: table => new
                {
                    MailAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncryptedRefreshToken = table.Column<string>(type: "text", nullable: false),
                    GrantedScopes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_oauth_credentials", x => x.MailAccountId);
                    table.ForeignKey(
                        name: "FK_oauth_credentials_mail_accounts_MailAccountId",
                        column: x => x.MailAccountId,
                        principalTable: "mail_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "provider_classification_targets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MailAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalTargetId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ExternalTargetName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_provider_classification_targets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_provider_classification_targets_mail_accounts_MailAccountId",
                        column: x => x.MailAccountId,
                        principalTable: "mail_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_provider_classification_targets_projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_mail_accounts_ConnectedByUserId",
                table: "mail_accounts",
                column: "ConnectedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_mail_accounts_OrganizationId_Provider_EmailAddress",
                table: "mail_accounts",
                columns: MailAccountUniqueIndexColumns,
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_provider_classification_targets_MailAccountId_ProjectId",
                table: "provider_classification_targets",
                columns: ClassificationTargetUniqueIndexColumns,
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_provider_classification_targets_ProjectId",
                table: "provider_classification_targets",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "oauth_credentials");

            migrationBuilder.DropTable(
                name: "provider_classification_targets");

            migrationBuilder.DropTable(
                name: "mail_accounts");
        }
    }
}
