using System.Collections.Generic;
using CoffeeChess.Domain.Games.ValueObjects;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeeChess.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddTimerInfoToMovesHistoryInCompletedGame : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<MoveInfo>>(
                name: "MovesHistory",
                table: "CompletedGames",
                type: "jsonb",
                nullable: true);

            migrationBuilder.Sql("""
                                 update "CompletedGames"
                                 set "MovesHistory" = coalesce(
                                     (
                                         select jsonb_agg(
                                             jsonb_build_object(
                                                 'san', move,
                                                 'timeAfterMove', '00:00:00'
                                             )
                                         )
                                         from unnest("SanMovesHistory") as move
                                     ),
                                     '[]'::jsonb
                                 );
                                 
                                 """);
            
            migrationBuilder.DropColumn(
                name: "SanMovesHistory",
                table: "CompletedGames");

            migrationBuilder.AlterColumn<List<MoveInfo>>(
                name: "MovesHistory",
                table: "CompletedGames",
                type: "jsonb",
                nullable: false,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "SanMovesHistory",
                table: "CompletedGames",
                type: "text[]",
                nullable: true);
            
            migrationBuilder.Sql("""
                                 update "CompletedGames" set "SanMovesHistory" = coalesce(
                                     (
                                        select array_agg(move->>'San')
                                        from jsonb_array_elements("MovesHistory") as move   
                                     ),
                                     '{}'::text[]
                                 );
                                 """);

            migrationBuilder.DropColumn(
                name: "MovesHistory",
                table: "CompletedGames");
            
            migrationBuilder.AlterColumn<List<string>>(
                name: "SanMovesHistory",
                table: "CompletedGames",
                type: "text[]",
                nullable: false,
                oldNullable: true);
        }
    }
}
