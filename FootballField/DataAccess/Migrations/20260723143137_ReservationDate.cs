using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ReservationDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FieldPriceSchedules_FootballFieldId",
                table: "FieldPriceSchedules");

            migrationBuilder.AddColumn<DateOnly>(
                name: "ReservationDate",
                table: "Reservations",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.CreateIndex(
                name: "IX_Unique_Field_Time_Day",
                table: "FieldPriceSchedules",
                columns: new[] { "FootballFieldId", "TimeSlotId", "DayId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Unique_Field_Time_Day",
                table: "FieldPriceSchedules");

            migrationBuilder.DropColumn(
                name: "ReservationDate",
                table: "Reservations");

            migrationBuilder.CreateIndex(
                name: "IX_FieldPriceSchedules_FootballFieldId",
                table: "FieldPriceSchedules",
                column: "FootballFieldId");
        }
    }
}
