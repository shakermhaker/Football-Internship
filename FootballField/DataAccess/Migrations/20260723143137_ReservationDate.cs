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
            // Sadece yeni kolonu ekliyoruz, index işlemleri diğer dosyada yapıldığı için buradan kaldırdık.
            migrationBuilder.AddColumn<DateOnly>(
                name: "ReservationDate",
                table: "Reservations",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Geri alırken de sadece kolonu siliyoruz.
            migrationBuilder.DropColumn(
                name: "ReservationDate",
                table: "Reservations");
        }
    }
}