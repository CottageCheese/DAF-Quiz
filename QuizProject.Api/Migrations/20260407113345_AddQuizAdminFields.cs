using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizProject.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizAdminFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Quizzes");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByEmail",
                table: "Quizzes",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Quizzes",
                type: "TEXT",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "Quizzes",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByEmail",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "Quizzes");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Quizzes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
