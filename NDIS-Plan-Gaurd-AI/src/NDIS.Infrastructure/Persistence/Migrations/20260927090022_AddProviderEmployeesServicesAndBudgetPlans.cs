using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NDIS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderEmployeesServicesAndBudgetPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ServiceDeliveryId",
                table: "Claims",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ServiceProviderEmployeeId",
                table: "Claims",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ParticipantBudgetPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TotalBudget = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DefaultFortnightBudget = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipantBudgetPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParticipantBudgetPlans_Participants_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "Participants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceProviderEmployees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceProviderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Qualifications = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultHourlyRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceProviderEmployees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceProviderEmployees_ServiceProviders_ServiceProviderId",
                        column: x => x.ServiceProviderId,
                        principalTable: "ServiceProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ParticipantBudgetAllocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParticipantBudgetPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupportCategory = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AllocatedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FortnightLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipantBudgetAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParticipantBudgetAllocations_ParticipantBudgetPlans_ParticipantBudgetPlanId",
                        column: x => x.ParticipantBudgetPlanId,
                        principalTable: "ParticipantBudgetPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParticipantServiceAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceProviderEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupportCategory = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    AgreedHourlyRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipantServiceAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParticipantServiceAssignments_Participants_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "Participants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParticipantServiceAssignments_ServiceProviderEmployees_ServiceProviderEmployeeId",
                        column: x => x.ServiceProviderEmployeeId,
                        principalTable: "ServiceProviderEmployees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceDeliveries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceProviderEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParticipantServiceAssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SupportCategory = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ServiceStartUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ServiceEndUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ServiceHours = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HourlyRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ServiceLocation = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCancelled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceDeliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceDeliveries_ParticipantServiceAssignments_ParticipantServiceAssignmentId",
                        column: x => x.ParticipantServiceAssignmentId,
                        principalTable: "ParticipantServiceAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceDeliveries_Participants_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "Participants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceDeliveries_ServiceProviderEmployees_ServiceProviderEmployeeId",
                        column: x => x.ServiceProviderEmployeeId,
                        principalTable: "ServiceProviderEmployees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Claims_ServiceDeliveryId",
                table: "Claims",
                column: "ServiceDeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_ServiceProviderEmployeeId",
                table: "Claims",
                column: "ServiceProviderEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantBudgetAllocations_ParticipantBudgetPlanId_SupportCategory",
                table: "ParticipantBudgetAllocations",
                columns: new[] { "ParticipantBudgetPlanId", "SupportCategory" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantBudgetPlans_ParticipantId",
                table: "ParticipantBudgetPlans",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantServiceAssignments_ParticipantId",
                table: "ParticipantServiceAssignments",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantServiceAssignments_ServiceProviderEmployeeId",
                table: "ParticipantServiceAssignments",
                column: "ServiceProviderEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceDeliveries_ParticipantId",
                table: "ServiceDeliveries",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceDeliveries_ParticipantServiceAssignmentId",
                table: "ServiceDeliveries",
                column: "ParticipantServiceAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceDeliveries_ServiceProviderEmployeeId_ServiceStartUtc_ServiceEndUtc",
                table: "ServiceDeliveries",
                columns: new[] { "ServiceProviderEmployeeId", "ServiceStartUtc", "ServiceEndUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceProviderEmployees_ServiceProviderId_EmployeeNumber",
                table: "ServiceProviderEmployees",
                columns: new[] { "ServiceProviderId", "EmployeeNumber" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_ServiceDeliveries_ServiceDeliveryId",
                table: "Claims",
                column: "ServiceDeliveryId",
                principalTable: "ServiceDeliveries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_ServiceProviderEmployees_ServiceProviderEmployeeId",
                table: "Claims",
                column: "ServiceProviderEmployeeId",
                principalTable: "ServiceProviderEmployees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Claims_ServiceDeliveries_ServiceDeliveryId",
                table: "Claims");

            migrationBuilder.DropForeignKey(
                name: "FK_Claims_ServiceProviderEmployees_ServiceProviderEmployeeId",
                table: "Claims");

            migrationBuilder.DropTable(
                name: "ParticipantBudgetAllocations");

            migrationBuilder.DropTable(
                name: "ServiceDeliveries");

            migrationBuilder.DropTable(
                name: "ParticipantBudgetPlans");

            migrationBuilder.DropTable(
                name: "ParticipantServiceAssignments");

            migrationBuilder.DropTable(
                name: "ServiceProviderEmployees");

            migrationBuilder.DropIndex(
                name: "IX_Claims_ServiceDeliveryId",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_ServiceProviderEmployeeId",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "ServiceDeliveryId",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "ServiceProviderEmployeeId",
                table: "Claims");
        }
    }
}
