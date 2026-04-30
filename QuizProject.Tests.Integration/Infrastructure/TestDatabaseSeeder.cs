using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using QuizProject.Domain.Data;
using QuizProject.Domain.Models.Domain;

namespace QuizProject.Tests.Integration.Infrastructure;

/// <summary>
/// Typed record returned by seeding so tests can reference known IDs without hitting the DB.
/// </summary>
public sealed record TestSeedContext(
    string AdminEmail,
    string AdminPassword,
    string UserEmail,
    string UserPassword,
    int PublishedQuizId,
    string PublishedQuizTitle,
    int[] QuestionIds,
    int[] CorrectAnswerIds,
    int[] WrongAnswerIds,
    int DraftQuizId,
    int CompletedAttemptId,
    string UserId
);

public static class TestDatabaseSeeder
{
    public const string AdminEmail = "admin@test.local";
    public const string AdminPassword = "Admin@123Test!";
    public const string UserEmail = "user@test.local";
    public const string UserPassword = "User@123Test!";
    public const string AdminDisplayName = "TestAdmin";
    public const string UserDisplayName = "TestUser";

    public static async Task<TestSeedContext> SeedAsync(IServiceProvider sp)
    {
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var db = sp.GetRequiredService<ApplicationDbContext>();

        // Roles
        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));

        // Admin user
        var admin = new ApplicationUser
        {
            UserName = AdminEmail,
            Email = AdminEmail,
            DisplayName = AdminDisplayName,
            EmailConfirmed = true
        };
        var adminResult = await userManager.CreateAsync(admin, AdminPassword);
        if (!adminResult.Succeeded)
            throw new InvalidOperationException($"Could not create admin: {string.Join(", ", adminResult.Errors.Select(e => e.Description))}");
        await userManager.AddToRoleAsync(admin, "Admin");

        // Regular user
        var user = new ApplicationUser
        {
            UserName = UserEmail,
            Email = UserEmail,
            DisplayName = UserDisplayName,
            EmailConfirmed = true
        };
        var userResult = await userManager.CreateAsync(user, UserPassword);
        if (!userResult.Succeeded)
            throw new InvalidOperationException($"Could not create user: {string.Join(", ", userResult.Errors.Select(e => e.Description))}");

        // Published quiz (3 questions, 4 answers each — 1 correct, 3 wrong)
        var publishedQuiz = new Quiz
        {
            Title = "Integration Test Quiz",
            Description = "Quiz for integration tests",
            CreatedByUserId = admin.Id,
            CreatedByEmail = AdminEmail,
            CreatedAt = DateTime.UtcNow,
            PublishedAt = DateTime.UtcNow.AddHours(-1)
        };

        db.Quizzes.Add(publishedQuiz);
        await db.SaveChangesAsync();

        var questionIds = new int[3];
        var correctAnswerIds = new int[3];
        var wrongAnswerIds = new int[3];

        for (var i = 0; i < 3; i++)
        {
            var question = new Question
            {
                QuizId = publishedQuiz.Id,
                Text = $"Test Question {i + 1}",
                DisplayOrder = i + 1
            };
            db.Questions.Add(question);
            await db.SaveChangesAsync();

            questionIds[i] = question.Id;

            var correctAnswer = new Answer { QuestionId = question.Id, Text = $"Correct Answer {i + 1}", IsCorrect = true };
            var wrongAnswer1 = new Answer { QuestionId = question.Id, Text = $"Wrong A {i + 1}", IsCorrect = false };
            var wrongAnswer2 = new Answer { QuestionId = question.Id, Text = $"Wrong B {i + 1}", IsCorrect = false };
            var wrongAnswer3 = new Answer { QuestionId = question.Id, Text = $"Wrong C {i + 1}", IsCorrect = false };

            db.Answers.AddRange(correctAnswer, wrongAnswer1, wrongAnswer2, wrongAnswer3);
            await db.SaveChangesAsync();

            correctAnswerIds[i] = correctAnswer.Id;
            wrongAnswerIds[i] = wrongAnswer1.Id;
        }

        // Draft quiz (no PublishedAt)
        var draftQuiz = new Quiz
        {
            Title = "Draft Quiz",
            Description = "Not published",
            CreatedByUserId = admin.Id,
            CreatedByEmail = AdminEmail,
            CreatedAt = DateTime.UtcNow
        };
        db.Quizzes.Add(draftQuiz);
        await db.SaveChangesAsync();

        // Pre-completed attempt for GetResult tests (score = 3/3)
        var completedAttempt = new QuizAttempt
        {
            UserId = user.Id,
            QuizId = publishedQuiz.Id,
            StartedAt = DateTime.UtcNow.AddMinutes(-5),
            CompletedAt = DateTime.UtcNow.AddMinutes(-1),
            Score = 3,
            TotalQuestions = 3
        };
        db.QuizAttempts.Add(completedAttempt);
        await db.SaveChangesAsync();

        for (var i = 0; i < 3; i++)
        {
            db.QuizAttemptAnswers.Add(new QuizAttemptAnswer
            {
                AttemptId = completedAttempt.Id,
                QuestionId = questionIds[i],
                SelectedAnswerId = correctAnswerIds[i],
                IsCorrect = true
            });
        }
        await db.SaveChangesAsync();

        return new TestSeedContext(
            AdminEmail: AdminEmail,
            AdminPassword: AdminPassword,
            UserEmail: UserEmail,
            UserPassword: UserPassword,
            PublishedQuizId: publishedQuiz.Id,
            PublishedQuizTitle: publishedQuiz.Title,
            QuestionIds: questionIds,
            CorrectAnswerIds: correctAnswerIds,
            WrongAnswerIds: wrongAnswerIds,
            DraftQuizId: draftQuiz.Id,
            CompletedAttemptId: completedAttempt.Id,
            UserId: user.Id
        );
    }
}
