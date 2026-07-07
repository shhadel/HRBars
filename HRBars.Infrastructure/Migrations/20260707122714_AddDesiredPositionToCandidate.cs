using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRBars.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDesiredVacancyToCandidate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DesiredVacancy",
                table: "Candidates",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DesiredVacancy",
                table: "Candidates");
        }
    }
}
