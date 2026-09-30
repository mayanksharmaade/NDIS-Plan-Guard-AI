using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NDIS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClaimDeliveriesAndBudgetPlanTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "TotalBudget",
                table: "ParticipantBudgetPlans",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddColumn<Guid>(
                name: "BudgetPlanTemplateId",
                table: "ParticipantBudgetPlans",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AgreedHourlyRate",
                table: "Claims",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalServiceHours",
                table: "Claims",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BudgetPlanTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    BudgetType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetPlanTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClaimServiceDeliveries",
                columns: table => new
                {
                    ClaimId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceDeliveryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimServiceDeliveries", x => new { x.ClaimId, x.ServiceDeliveryId });
                    table.ForeignKey(
                        name: "FK_ClaimServiceDeliveries_Claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "Claims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClaimServiceDeliveries_ServiceDeliveries_ServiceDeliveryId",
                        column: x => x.ServiceDeliveryId,
                        principalTable: "ServiceDeliveries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantBudgetPlans_BudgetPlanTemplateId",
                table: "ParticipantBudgetPlans",
                column: "BudgetPlanTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetPlanTemplates_PlanName",
                table: "BudgetPlanTemplates",
                column: "PlanName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClaimServiceDeliveries_ServiceDeliveryId",
                table: "ClaimServiceDeliveries",
                column: "ServiceDeliveryId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ParticipantBudgetPlans_BudgetPlanTemplates_BudgetPlanTemplateId",
                table: "ParticipantBudgetPlans",
                column: "BudgetPlanTemplateId",
                principalTable: "BudgetPlanTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParticipantBudgetPlans_BudgetPlanTemplates_BudgetPlanTemplateId",
                table: "ParticipantBudgetPlans");

            migrationBuilder.DropTable(
                name: "BudgetPlanTemplates");

            migrationBuilder.DropTable(
                name: "ClaimServiceDeliveries");

            migrationBuilder.DropIndex(
                name: "IX_ParticipantBudgetPlans_BudgetPlanTemplateId",
                table: "ParticipantBudgetPlans");

            migrationBuilder.DropColumn(
                name: "BudgetPlanTemplateId",
                table: "ParticipantBudgetPlans");

            migrationBuilder.DropColumn(
                name: "AgreedHourlyRate",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "TotalServiceHours",
                table: "Claims");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalBudget",
                table: "ParticipantBudgetPlans",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);
        }
    }
}
