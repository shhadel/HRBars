using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRBars.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVacancyTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "EmploymentType",
                table: "Vacancies",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "ExperienceRequired",
                table: "Vacancies",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<decimal>(
                name: "SalaryFrom",
                table: "Vacancies",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SalaryTo",
                table: "Vacancies",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmploymentType",
                table: "Vacancies");

            migrationBuilder.DropColumn(
                name: "ExperienceRequired",
                table: "Vacancies");

            migrationBuilder.DropColumn(
                name: "SalaryFrom",
                table: "Vacancies");

            migrationBuilder.DropColumn(
                name: "SalaryTo",
                table: "Vacancies");
        }
    }
}
