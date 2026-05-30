#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace QuizProject.Domain.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            "AspNetRoles",
            table => new
            {
                Id = table.Column<string>("nvarchar(450)", nullable: false),
                Name = table.Column<string>("nvarchar(256)", maxLength: 256, nullable: true),
                NormalizedName = table.Column<string>("nvarchar(256)", maxLength: 256, nullable: true),
                ConcurrencyStamp = table.Column<string>("nvarchar(max)", nullable: true)
            },
            constraints: table => { table.PrimaryKey("PK_AspNetRoles", x => x.Id); });

        migrationBuilder.CreateTable(
            "AspNetUsers",
            table => new
            {
                Id = table.Column<string>("nvarchar(450)", nullable: false),
                DisplayName = table.Column<string>("nvarchar(50)", maxLength: 50, nullable: false),
                UserName = table.Column<string>("nvarchar(256)", maxLength: 256, nullable: true),
                NormalizedUserName = table.Column<string>("nvarchar(256)", maxLength: 256, nullable: true),
                Email = table.Column<string>("nvarchar(256)", maxLength: 256, nullable: true),
                NormalizedEmail = table.Column<string>("nvarchar(256)", maxLength: 256, nullable: true),
                EmailConfirmed = table.Column<bool>("bit", nullable: false),
                PasswordHash = table.Column<string>("nvarchar(max)", nullable: true),
                SecurityStamp = table.Column<string>("nvarchar(max)", nullable: true),
                ConcurrencyStamp = table.Column<string>("nvarchar(max)", nullable: true),
                PhoneNumber = table.Column<string>("nvarchar(max)", nullable: true),
                PhoneNumberConfirmed = table.Column<bool>("bit", nullable: false),
                TwoFactorEnabled = table.Column<bool>("bit", nullable: false),
                LockoutEnd = table.Column<DateTimeOffset>("datetimeoffset", nullable: true),
                LockoutEnabled = table.Column<bool>("bit", nullable: false),
                AccessFailedCount = table.Column<int>("int", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_AspNetUsers", x => x.Id); });

        migrationBuilder.CreateTable(
            "Quizzes",
            table => new
            {
                Id = table.Column<int>("int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Title = table.Column<string>("nvarchar(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>("nvarchar(1000)", maxLength: 1000, nullable: true),
                CreatedAt = table.Column<DateTime>("datetime2", nullable: false),
                CreatedByUserId = table.Column<string>("nvarchar(450)", maxLength: 450, nullable: true),
                CreatedByEmail = table.Column<string>("nvarchar(256)", maxLength: 256, nullable: true),
                PublishedAt = table.Column<DateTime>("datetime2", nullable: true)
            },
            constraints: table => { table.PrimaryKey("PK_Quizzes", x => x.Id); });

        migrationBuilder.CreateTable(
            "AspNetRoleClaims",
            table => new
            {
                Id = table.Column<int>("int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                RoleId = table.Column<string>("nvarchar(450)", nullable: false),
                ClaimType = table.Column<string>("nvarchar(max)", nullable: true),
                ClaimValue = table.Column<string>("nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                table.ForeignKey(
                    "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                    x => x.RoleId,
                    "AspNetRoles",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            "AspNetUserClaims",
            table => new
            {
                Id = table.Column<int>("int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<string>("nvarchar(450)", nullable: false),
                ClaimType = table.Column<string>("nvarchar(max)", nullable: true),
                ClaimValue = table.Column<string>("nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                table.ForeignKey(
                    "FK_AspNetUserClaims_AspNetUsers_UserId",
                    x => x.UserId,
                    "AspNetUsers",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            "AspNetUserLogins",
            table => new
            {
                LoginProvider = table.Column<string>("nvarchar(450)", nullable: false),
                ProviderKey = table.Column<string>("nvarchar(450)", nullable: false),
                ProviderDisplayName = table.Column<string>("nvarchar(max)", nullable: true),
                UserId = table.Column<string>("nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                table.ForeignKey(
                    "FK_AspNetUserLogins_AspNetUsers_UserId",
                    x => x.UserId,
                    "AspNetUsers",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            "AspNetUserRoles",
            table => new
            {
                UserId = table.Column<string>("nvarchar(450)", nullable: false),
                RoleId = table.Column<string>("nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                table.ForeignKey(
                    "FK_AspNetUserRoles_AspNetRoles_RoleId",
                    x => x.RoleId,
                    "AspNetRoles",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    "FK_AspNetUserRoles_AspNetUsers_UserId",
                    x => x.UserId,
                    "AspNetUsers",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            "AspNetUserTokens",
            table => new
            {
                UserId = table.Column<string>("nvarchar(450)", nullable: false),
                LoginProvider = table.Column<string>("nvarchar(450)", nullable: false),
                Name = table.Column<string>("nvarchar(450)", nullable: false),
                Value = table.Column<string>("nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                table.ForeignKey(
                    "FK_AspNetUserTokens_AspNetUsers_UserId",
                    x => x.UserId,
                    "AspNetUsers",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            "RefreshTokens",
            table => new
            {
                Id = table.Column<int>("int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                TokenHash = table.Column<string>("nvarchar(64)", maxLength: 64, nullable: false),
                UserId = table.Column<string>("nvarchar(450)", nullable: false),
                CreatedAt = table.Column<DateTime>("datetime2", nullable: false),
                ExpiresAt = table.Column<DateTime>("datetime2", nullable: false),
                UsedAt = table.Column<DateTime>("datetime2", nullable: true),
                RevokedAt = table.Column<DateTime>("datetime2", nullable: true),
                ReplacedByTokenHash = table.Column<string>("nvarchar(64)", maxLength: 64, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                table.ForeignKey(
                    "FK_RefreshTokens_AspNetUsers_UserId",
                    x => x.UserId,
                    "AspNetUsers",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            "Questions",
            table => new
            {
                Id = table.Column<int>("int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                QuizId = table.Column<int>("int", nullable: false),
                Text = table.Column<string>("nvarchar(1000)", maxLength: 1000, nullable: false),
                DisplayOrder = table.Column<int>("int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Questions", x => x.Id);
                table.ForeignKey(
                    "FK_Questions_Quizzes_QuizId",
                    x => x.QuizId,
                    "Quizzes",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            "QuizAttempts",
            table => new
            {
                Id = table.Column<int>("int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<string>("nvarchar(450)", nullable: false),
                QuizId = table.Column<int>("int", nullable: false),
                StartedAt = table.Column<DateTime>("datetime2", nullable: false),
                CompletedAt = table.Column<DateTime>("datetime2", nullable: true),
                Score = table.Column<int>("int", nullable: false),
                TotalQuestions = table.Column<int>("int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_QuizAttempts", x => x.Id);
                table.ForeignKey(
                    "FK_QuizAttempts_AspNetUsers_UserId",
                    x => x.UserId,
                    "AspNetUsers",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    "FK_QuizAttempts_Quizzes_QuizId",
                    x => x.QuizId,
                    "Quizzes",
                    "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            "Answers",
            table => new
            {
                Id = table.Column<int>("int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                QuestionId = table.Column<int>("int", nullable: false),
                Text = table.Column<string>("nvarchar(500)", maxLength: 500, nullable: false),
                IsCorrect = table.Column<bool>("bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Answers", x => x.Id);
                table.ForeignKey(
                    "FK_Answers_Questions_QuestionId",
                    x => x.QuestionId,
                    "Questions",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            "QuizAttemptAnswers",
            table => new
            {
                Id = table.Column<int>("int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                AttemptId = table.Column<int>("int", nullable: false),
                QuestionId = table.Column<int>("int", nullable: false),
                SelectedAnswerId = table.Column<int>("int", nullable: false),
                IsCorrect = table.Column<bool>("bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_QuizAttemptAnswers", x => x.Id);
                table.ForeignKey(
                    "FK_QuizAttemptAnswers_Answers_SelectedAnswerId",
                    x => x.SelectedAnswerId,
                    "Answers",
                    "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    "FK_QuizAttemptAnswers_Questions_QuestionId",
                    x => x.QuestionId,
                    "Questions",
                    "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    "FK_QuizAttemptAnswers_QuizAttempts_AttemptId",
                    x => x.AttemptId,
                    "QuizAttempts",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            "IX_Answers_QuestionId",
            "Answers",
            "QuestionId");

        migrationBuilder.CreateIndex(
            "IX_AspNetRoleClaims_RoleId",
            "AspNetRoleClaims",
            "RoleId");

        migrationBuilder.CreateIndex(
            "RoleNameIndex",
            "AspNetRoles",
            "NormalizedName",
            unique: true,
            filter: "[NormalizedName] IS NOT NULL");

        migrationBuilder.CreateIndex(
            "IX_AspNetUserClaims_UserId",
            "AspNetUserClaims",
            "UserId");

        migrationBuilder.CreateIndex(
            "IX_AspNetUserLogins_UserId",
            "AspNetUserLogins",
            "UserId");

        migrationBuilder.CreateIndex(
            "IX_AspNetUserRoles_RoleId",
            "AspNetUserRoles",
            "RoleId");

        migrationBuilder.CreateIndex(
            "EmailIndex",
            "AspNetUsers",
            "NormalizedEmail");

        migrationBuilder.CreateIndex(
            "IX_AspNetUsers_DisplayName",
            "AspNetUsers",
            "DisplayName",
            unique: true);

        migrationBuilder.CreateIndex(
            "UserNameIndex",
            "AspNetUsers",
            "NormalizedUserName",
            unique: true,
            filter: "[NormalizedUserName] IS NOT NULL");

        migrationBuilder.CreateIndex(
            "IX_Questions_QuizId",
            "Questions",
            "QuizId");

        migrationBuilder.CreateIndex(
            "IX_QuizAttemptAnswers_AttemptId",
            "QuizAttemptAnswers",
            "AttemptId");

        migrationBuilder.CreateIndex(
            "IX_QuizAttemptAnswers_QuestionId",
            "QuizAttemptAnswers",
            "QuestionId");

        migrationBuilder.CreateIndex(
            "IX_QuizAttemptAnswers_SelectedAnswerId",
            "QuizAttemptAnswers",
            "SelectedAnswerId");

        migrationBuilder.CreateIndex(
            "IX_QuizAttempts_CompletedAt",
            "QuizAttempts",
            "CompletedAt");

        migrationBuilder.CreateIndex(
            "IX_QuizAttempts_QuizId",
            "QuizAttempts",
            "QuizId");

        migrationBuilder.CreateIndex(
            "IX_QuizAttempts_UserId",
            "QuizAttempts",
            "UserId");

        migrationBuilder.CreateIndex(
            "IX_Quizzes_PublishedAt",
            "Quizzes",
            "PublishedAt");

        migrationBuilder.CreateIndex(
            "IX_RefreshTokens_TokenHash",
            "RefreshTokens",
            "TokenHash",
            unique: true);

        migrationBuilder.CreateIndex(
            "IX_RefreshTokens_UserId",
            "RefreshTokens",
            "UserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            "AspNetRoleClaims");

        migrationBuilder.DropTable(
            "AspNetUserClaims");

        migrationBuilder.DropTable(
            "AspNetUserLogins");

        migrationBuilder.DropTable(
            "AspNetUserRoles");

        migrationBuilder.DropTable(
            "AspNetUserTokens");

        migrationBuilder.DropTable(
            "QuizAttemptAnswers");

        migrationBuilder.DropTable(
            "RefreshTokens");

        migrationBuilder.DropTable(
            "AspNetRoles");

        migrationBuilder.DropTable(
            "Answers");

        migrationBuilder.DropTable(
            "QuizAttempts");

        migrationBuilder.DropTable(
            "Questions");

        migrationBuilder.DropTable(
            "AspNetUsers");

        migrationBuilder.DropTable(
            "Quizzes");
    }
}