using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeeChess.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddCompletedGamesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompletedGames",
                columns: table => new
                {
                    GameId = table.Column<string>(type: "text", nullable: false),
                    WhitePlayerId = table.Column<string>(type: "text", nullable: false),
                    WhitePlayerName = table.Column<string>(type: "text", nullable: false),
                    WhitePlayerRating = table.Column<int>(type: "integer", nullable: false),
                    WhitePlayerNewRating = table.Column<int>(type: "integer", nullable: false),
                    BlackPlayerId = table.Column<string>(type: "text", nullable: false),
                    BlackPlayerName = table.Column<string>(type: "text", nullable: false),
                    BlackPlayerRating = table.Column<int>(type: "integer", nullable: false),
                    BlackPlayerNewRating = table.Column<int>(type: "integer", nullable: false),
                    GameResult = table.Column<int>(type: "integer", nullable: false),
                    PlayedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Pgn = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletedGames", x => x.GameId);
                    table.ForeignKey(
                        name: "FK_CompletedGames_Players_BlackPlayerId",
                        column: x => x.BlackPlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompletedGames_Players_WhitePlayerId",
                        column: x => x.WhitePlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompletedGames_BlackPlayerId",
                table: "CompletedGames",
                column: "BlackPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_CompletedGames_WhitePlayerId",
                table: "CompletedGames",
                column: "WhitePlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompletedGames");
        }
    }
}
