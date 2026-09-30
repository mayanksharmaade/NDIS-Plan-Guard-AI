using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NDIS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClaimWorkflowReviewAuditDashboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DecidedAtUtc",
                table: "Claims",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewStartedAtUtc",
                table: "Claims",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuditEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceProviderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ActorIdentityUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ActorEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClaimReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewerIdentityUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewerEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Outcome = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReviewedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClaimReviews_Claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "Claims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Claims_Status_SubmittedAtUtc",
                table: "Claims",
                columns: new[] { "Status", "SubmittedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_ActorIdentityUserId",
                table: "AuditEvents",
                column: "ActorIdentityUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_EntityType_EntityId_OccurredAtUtc",
                table: "AuditEvents",
                columns: new[] { "EntityType", "EntityId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_ServiceProviderId_OccurredAtUtc",
                table: "AuditEvents",
                columns: new[] { "ServiceProviderId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ClaimReviews_ClaimId_ReviewedAtUtc",
                table: "ClaimReviews",
                columns: new[] { "ClaimId", "ReviewedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ClaimReviews_ReviewerIdentityUserId",
                table: "ClaimReviews",
                column: "ReviewerIdentityUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditEvents");

            migrationBuilder.DropTable(
                name: "ClaimReviews");

            migrationBuilder.DropIndex(
                name: "IX_Claims_Status_SubmittedAtUtc",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "DecidedAtUtc",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "ReviewStartedAtUtc",
                table: "Claims");
        }
    }
}
