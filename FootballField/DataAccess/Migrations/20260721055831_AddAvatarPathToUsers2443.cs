using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarPathToUsers2443 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.AlterColumn<Guid>(
                name: "RowGuid",
                table: "Users",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<int>(
                name: "TeamAvatarId",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TeamAvatars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TeamName = table.Column<string>(type: "text", nullable: false),
                    ImagePath = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamAvatars", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_TeamAvatarId",
                table: "Users",
                column: "TeamAvatarId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_TeamAvatars_TeamAvatarId",
                table: "Users",
                column: "TeamAvatarId",
                principalTable: "TeamAvatars",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_TeamAvatars_TeamAvatarId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "TeamAvatars");

            migrationBuilder.DropIndex(
                name: "IX_Users_TeamAvatarId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TeamAvatarId",
                table: "Users");

            migrationBuilder.AlterColumn<Guid>(
                name: "RowGuid",
                table: "Users",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

           
        }
    }
}
