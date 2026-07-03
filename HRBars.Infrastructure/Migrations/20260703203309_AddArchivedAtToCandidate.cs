using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRBars.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddArchivedAtToCandidate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAt",
                table: "Candidates",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "Candidates");
        }
    }
}
