using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizProject.Api.Migrations
{
    /// <inheritdoc />
    [Migration("20260407120000_AddPerformanceIndexes")]
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_CompletedAt",
                table: "QuizAttempts",
                column: "CompletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_PublishedAt",
                table: "Quizzes",
                column: "PublishedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QuizAttempts_CompletedAt",
                table: "QuizAttempts");

            migrationBuilder.DropIndex(
                name: "IX_Quizzes_PublishedAt",
                table: "Quizzes");
        }
    }
}
