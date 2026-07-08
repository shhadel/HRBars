using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRBars.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Users_CreatedByUserId",
                table: "Applications");

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAt",
                table: "Vacancies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ArchivedByUserId",
                table: "Vacancies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Vacancies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Vacancies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "Vacancies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedByUserId",
                table: "Interviews",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "ArchivedByUserId",
                table: "Interviews",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Interviews",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "Interviews",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ArchivedByUserId",
                table: "Candidates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Candidates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "Candidates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedByUserId",
                table: "Applications",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAt",
                table: "Applications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ArchivedByUserId",
                table: "Applications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Applications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "Applications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Applications",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vacancies_ArchivedByUserId",
                table: "Vacancies",
                column: "ArchivedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Vacancies_CreatedByUserId",
                table: "Vacancies",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Vacancies_UpdatedByUserId",
                table: "Vacancies",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedByUserId",
                table: "Users",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UpdatedByUserId",
                table: "Users",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_ArchivedByUserId",
                table: "Interviews",
                column: "ArchivedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_UpdatedByUserId",
                table: "Interviews",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Candidates_ArchivedByUserId",
                table: "Candidates",
                column: "ArchivedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Candidates_CreatedByUserId",
                table: "Candidates",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Candidates_UpdatedByUserId",
                table: "Candidates",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ArchivedByUserId",
                table: "Applications",
                column: "ArchivedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_UpdatedByUserId",
                table: "Applications",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_UserId",
                table: "Applications",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Users_ArchivedByUserId",
                table: "Applications",
                column: "ArchivedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Users_CreatedByUserId",
                table: "Applications",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Users_UpdatedByUserId",
                table: "Applications",
                column: "UpdatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Users_UserId",
                table: "Applications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Candidates_Users_ArchivedByUserId",
                table: "Candidates",
                column: "ArchivedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Candidates_Users_CreatedByUserId",
                table: "Candidates",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Candidates_Users_UpdatedByUserId",
                table: "Candidates",
                column: "UpdatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Interviews_Users_ArchivedByUserId",
                table: "Interviews",
                column: "ArchivedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Interviews_Users_UpdatedByUserId",
                table: "Interviews",
                column: "UpdatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_CreatedByUserId",
                table: "Users",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_UpdatedByUserId",
                table: "Users",
                column: "UpdatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vacancies_Users_ArchivedByUserId",
                table: "Vacancies",
                column: "ArchivedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vacancies_Users_CreatedByUserId",
                table: "Vacancies",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vacancies_Users_UpdatedByUserId",
                table: "Vacancies",
                column: "UpdatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Users_ArchivedByUserId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Users_CreatedByUserId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Users_UpdatedByUserId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Users_UserId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_Candidates_Users_ArchivedByUserId",
                table: "Candidates");

            migrationBuilder.DropForeignKey(
                name: "FK_Candidates_Users_CreatedByUserId",
                table: "Candidates");

            migrationBuilder.DropForeignKey(
                name: "FK_Candidates_Users_UpdatedByUserId",
                table: "Candidates");

            migrationBuilder.DropForeignKey(
                name: "FK_Interviews_Users_ArchivedByUserId",
                table: "Interviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Interviews_Users_UpdatedByUserId",
                table: "Interviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_CreatedByUserId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_UpdatedByUserId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Vacancies_Users_ArchivedByUserId",
                table: "Vacancies");

            migrationBuilder.DropForeignKey(
                name: "FK_Vacancies_Users_CreatedByUserId",
                table: "Vacancies");

            migrationBuilder.DropForeignKey(
                name: "FK_Vacancies_Users_UpdatedByUserId",
                table: "Vacancies");

            migrationBuilder.DropIndex(
                name: "IX_Vacancies_ArchivedByUserId",
                table: "Vacancies");

            migrationBuilder.DropIndex(
                name: "IX_Vacancies_CreatedByUserId",
                table: "Vacancies");

            migrationBuilder.DropIndex(
                name: "IX_Vacancies_UpdatedByUserId",
                table: "Vacancies");

            migrationBuilder.DropIndex(
                name: "IX_Users_CreatedByUserId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_UpdatedByUserId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Interviews_ArchivedByUserId",
                table: "Interviews");

            migrationBuilder.DropIndex(
                name: "IX_Interviews_UpdatedByUserId",
                table: "Interviews");

            migrationBuilder.DropIndex(
                name: "IX_Candidates_ArchivedByUserId",
                table: "Candidates");

            migrationBuilder.DropIndex(
                name: "IX_Candidates_CreatedByUserId",
                table: "Candidates");

            migrationBuilder.DropIndex(
                name: "IX_Candidates_UpdatedByUserId",
                table: "Candidates");

            migrationBuilder.DropIndex(
                name: "IX_Applications_ArchivedByUserId",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_UpdatedByUserId",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_UserId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "Vacancies");

            migrationBuilder.DropColumn(
                name: "ArchivedByUserId",
                table: "Vacancies");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Vacancies");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Vacancies");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Vacancies");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ArchivedByUserId",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "ArchivedByUserId",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "ArchivedByUserId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Applications");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedByUserId",
                table: "Interviews",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedByUserId",
                table: "Applications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Users_CreatedByUserId",
                table: "Applications",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
