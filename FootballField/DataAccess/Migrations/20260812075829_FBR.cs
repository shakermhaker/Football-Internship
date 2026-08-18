using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FBR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FreeBookingRights_FootballFields_FootballFieldId",
                table: "FreeBookingRights");

            migrationBuilder.RenameColumn(
                name: "RemainingUsageCount",
                table: "FreeBookingRights",
                newName: "ReservationId");

            migrationBuilder.RenameColumn(
                name: "FootballFieldId",
                table: "FreeBookingRights",
                newName: "BusinessId");

            migrationBuilder.RenameIndex(
                name: "IX_FreeBookingRights_FootballFieldId",
                table: "FreeBookingRights",
                newName: "IX_FreeBookingRights_BusinessId");

            migrationBuilder.AddColumn<bool>(
                name: "IsUsed",
                table: "FreeBookingRights",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_FreeBookingRights_ReservationId",
                table: "FreeBookingRights",
                column: "ReservationId");

            migrationBuilder.AddForeignKey(
                name: "FK_FreeBookingRights_Businesses_BusinessId",
                table: "FreeBookingRights",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FreeBookingRights_Reservations_ReservationId",
                table: "FreeBookingRights",
                column: "ReservationId",
                principalTable: "Reservations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FreeBookingRights_Businesses_BusinessId",
                table: "FreeBookingRights");

            migrationBuilder.DropForeignKey(
                name: "FK_FreeBookingRights_Reservations_ReservationId",
                table: "FreeBookingRights");

            migrationBuilder.DropIndex(
                name: "IX_FreeBookingRights_ReservationId",
                table: "FreeBookingRights");

            migrationBuilder.DropColumn(
                name: "IsUsed",
                table: "FreeBookingRights");

            migrationBuilder.RenameColumn(
                name: "ReservationId",
                table: "FreeBookingRights",
                newName: "RemainingUsageCount");

            migrationBuilder.RenameColumn(
                name: "BusinessId",
                table: "FreeBookingRights",
                newName: "FootballFieldId");

            migrationBuilder.RenameIndex(
                name: "IX_FreeBookingRights_BusinessId",
                table: "FreeBookingRights",
                newName: "IX_FreeBookingRights_FootballFieldId");

            migrationBuilder.AddForeignKey(
                name: "FK_FreeBookingRights_FootballFields_FootballFieldId",
                table: "FreeBookingRights",
                column: "FootballFieldId",
                principalTable: "FootballFields",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
