using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NDIS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddParticipantEmergencyContact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactName",
                table: "Participants",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactPhoneNumber",
                table: "Participants",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactRelationship",
                table: "Participants",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmergencyContactName",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "EmergencyContactPhoneNumber",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "EmergencyContactRelationship",
                table: "Participants");
        }
    }
}
