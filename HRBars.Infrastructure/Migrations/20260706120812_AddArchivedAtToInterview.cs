using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRBars.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddArchivedAtToInterview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAt",
                table: "Interviews",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "Interviews");
        }
    }
}
