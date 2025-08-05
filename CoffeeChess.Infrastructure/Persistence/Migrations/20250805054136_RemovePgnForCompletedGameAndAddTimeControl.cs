using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeeChess.Web.Migrations
{
    /// <inheritdoc />
    public partial class RemovePgnForCompletedGameAndAddTimeControl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Pgn",
                table: "CompletedGames");

            migrationBuilder.AddColumn<int>(
                name: "Increment",
                table: "CompletedGames",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Minutes",
                table: "CompletedGames",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Increment",
                table: "CompletedGames");

            migrationBuilder.DropColumn(
                name: "Minutes",
                table: "CompletedGames");

            migrationBuilder.AddColumn<string>(
                name: "Pgn",
                table: "CompletedGames",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
