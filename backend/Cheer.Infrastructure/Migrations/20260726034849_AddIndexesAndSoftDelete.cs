using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cheer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesAndSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Teams",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ChampionshipId",
                table: "CompetitionResults",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_Categoria",
                table: "Teams",
                column: "Categoria");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_Cidade",
                table: "Teams",
                column: "Cidade");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_Nivel",
                table: "Teams",
                column: "Nivel");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_Score",
                table: "Teams",
                column: "Score");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_Status",
                table: "Teams",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionResults_Ano",
                table: "CompetitionResults",
                column: "Ano");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionResults_ChampionshipId",
                table: "CompetitionResults",
                column: "ChampionshipId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionResults_Championships_ChampionshipId",
                table: "CompetitionResults",
                column: "ChampionshipId",
                principalTable: "Championships",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionResults_Championships_ChampionshipId",
                table: "CompetitionResults");

            migrationBuilder.DropIndex(
                name: "IX_Teams_Categoria",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_Cidade",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_Nivel",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_Score",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_Status",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_CompetitionResults_Ano",
                table: "CompetitionResults");

            migrationBuilder.DropIndex(
                name: "IX_CompetitionResults_ChampionshipId",
                table: "CompetitionResults");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "ChampionshipId",
                table: "CompetitionResults");
        }
    }
}
