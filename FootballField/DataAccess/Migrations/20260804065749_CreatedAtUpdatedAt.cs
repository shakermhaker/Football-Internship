using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class CreatedAtUpdatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "UserOperationClaims",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "UserOperationClaims",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "UserOperationClaims",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "UserOperationClaims",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TimeSlots",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "TimeSlots",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TimeSlots",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "TimeSlots",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TeamAvatars",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "TeamAvatars",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TeamAvatars",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "TeamAvatars",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Statuses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Statuses",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Statuses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "Statuses",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Reservations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Reservations",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Reservations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "Reservations",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "OperationClaims",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "OperationClaims",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "OperationClaims",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "OperationClaims",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "FootballFields",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "FootballFields",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "FootballFields",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "FootballFields",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "FieldPriceSchedules",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "FieldPriceSchedules",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "FieldPriceSchedules",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "FieldPriceSchedules",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Districts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Districts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Districts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "Districts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Days",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Days",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Days",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "Days",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Cities",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Cities",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Cities",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "Cities",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "BusinessImages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "BusinessImages",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "BusinessImages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "BusinessImages",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Businesses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Businesses",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Businesses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "Businesses",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "UserOperationClaims");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "UserOperationClaims");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "UserOperationClaims");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "UserOperationClaims");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TimeSlots");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TimeSlots");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TimeSlots");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TimeSlots");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TeamAvatars");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TeamAvatars");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TeamAvatars");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TeamAvatars");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Statuses");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Statuses");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Statuses");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Statuses");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "OperationClaims");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "OperationClaims");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "OperationClaims");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "OperationClaims");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "FootballFields");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "FootballFields");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "FootballFields");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "FootballFields");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "FieldPriceSchedules");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "FieldPriceSchedules");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "FieldPriceSchedules");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "FieldPriceSchedules");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Districts");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Districts");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Districts");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Districts");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Days");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Days");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Days");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Days");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "BusinessImages");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "BusinessImages");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "BusinessImages");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "BusinessImages");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Businesses");
        }
    }
}
