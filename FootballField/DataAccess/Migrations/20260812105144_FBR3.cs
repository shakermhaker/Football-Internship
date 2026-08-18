using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FBR3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Unique_ReservationDate_ScheduleId",
                table: "Reservations");

            migrationBuilder.CreateIndex(
                name: "IX_Unique_ReservationDate_ScheduleId",
                table: "Reservations",
                columns: new[] { "ReservationDate", "FieldPriceScheduleId" },
                unique: true,
                filter: "\"IsDeleted\" = false AND \"StatusId\" = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Unique_ReservationDate_ScheduleId",
                table: "Reservations");

            migrationBuilder.CreateIndex(
                name: "IX_Unique_ReservationDate_ScheduleId",
                table: "Reservations",
                columns: new[] { "ReservationDate", "FieldPriceScheduleId" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }
    }
}
