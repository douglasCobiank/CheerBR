using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cheer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveParticipantes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Participantes",
                table: "CompetitionResults");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Participantes",
                table: "CompetitionResults",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
