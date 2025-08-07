using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeeChess.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddResultReasonToCompletedGameReadModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GameResultReason",
                table: "CompletedGames",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GameResultReason",
                table: "CompletedGames");
        }
    }
}
