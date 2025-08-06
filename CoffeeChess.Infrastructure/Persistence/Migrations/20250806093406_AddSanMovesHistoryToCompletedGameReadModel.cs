using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeeChess.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddSanMovesHistoryToCompletedGameReadModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "SanMovesHistory",
                table: "CompletedGames",
                type: "text[]",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SanMovesHistory",
                table: "CompletedGames");
        }
    }
}
