using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FB2R : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Unique_ReservationDate_ScheduleId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Unique_Field_Time_Day",
                table: "FieldPriceSchedules");

            migrationBuilder.CreateIndex(
                name: "IX_Unique_ReservationDate_ScheduleId",
                table: "Reservations",
                columns: new[] { "ReservationDate", "FieldPriceScheduleId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Unique_Field_Time_Day",
                table: "FieldPriceSchedules",
                columns: new[] { "FootballFieldId", "TimeSlotId", "DayId" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Unique_ReservationDate_ScheduleId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Unique_Field_Time_Day",
                table: "FieldPriceSchedules");

            migrationBuilder.CreateIndex(
                name: "IX_Unique_ReservationDate_ScheduleId",
                table: "Reservations",
                columns: new[] { "ReservationDate", "FieldPriceScheduleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Unique_Field_Time_Day",
                table: "FieldPriceSchedules",
                columns: new[] { "FootballFieldId", "TimeSlotId", "DayId" },
                unique: true);
        }
    }
}
