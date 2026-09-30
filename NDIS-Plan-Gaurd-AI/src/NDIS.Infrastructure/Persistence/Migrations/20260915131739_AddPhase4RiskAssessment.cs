using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NDIS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPhase4RiskAssessment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PlanTotalBudget",
                table: "Participants",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "Claims",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Units",
                table: "Claims",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClaimRiskAssessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Available = table.Column<bool>(type: "bit", nullable: false),
                    Probability = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    Score = table.Column<int>(type: "int", nullable: true),
                    RiskBand = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ModelVersion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FeatureVersion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ScoredAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimRiskAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClaimRiskAssessments_Claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "Claims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClaimRiskFactors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimRiskAssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Feature = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ObservedValueJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    BaselineValueJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ProbabilityImpact = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimRiskFactors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClaimRiskFactors_ClaimRiskAssessments_ClaimRiskAssessmentId",
                        column: x => x.ClaimRiskAssessmentId,
                        principalTable: "ClaimRiskAssessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClaimRiskAssessments_ClaimId_CreatedAtUtc",
                table: "ClaimRiskAssessments",
                columns: new[] { "ClaimId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ClaimRiskFactors_ClaimRiskAssessmentId",
                table: "ClaimRiskFactors",
                column: "ClaimRiskAssessmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClaimRiskFactors");

            migrationBuilder.DropTable(
                name: "ClaimRiskAssessments");

            migrationBuilder.DropColumn(
                name: "PlanTotalBudget",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "Units",
                table: "Claims");
        }
    }
}
