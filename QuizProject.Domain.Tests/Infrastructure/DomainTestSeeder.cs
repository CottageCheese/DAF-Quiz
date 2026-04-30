using QuizProject.Domain.Data;
using QuizProject.Domain.Models.Domain;

namespace QuizProject.Domain.Tests.Infrastructure;

public static class DomainTestSeeder
{
    public static async Task<SeedContext> SeedAsync(ApplicationDbContext db)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "testuser@test.local",
            NormalizedUserName = "TESTUSER@TEST.LOCAL",
            Email = "testuser@test.local",
            NormalizedEmail = "TESTUSER@TEST.LOCAL",
            DisplayName = "TestUser",
            SecurityStamp = Guid.NewGuid().ToString()
        };
        db.Users.Add(user);

        var publishedQuiz = new Quiz
        {
            Title = "Published Quiz",
            Description = "A published quiz",
            CreatedByUserId = user.Id,
            CreatedByEmail = user.Email!,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            PublishedAt = DateTime.UtcNow.AddHours(-1)
        };
        var draftQuiz = new Quiz
        {
            Title = "Draft Quiz",
            Description = "Not published",
            CreatedByUserId = user.Id,
            CreatedByEmail = user.Email!,
            CreatedAt = DateTime.UtcNow
        };
        db.Quizzes.AddRange(publishedQuiz, draftQuiz);
        await db.SaveChangesAsync();

        var questionIds = new int[2];
        var correctAnswerIds = new int[2];
        var wrongAnswerIds = new int[2];

        for (var i = 0; i < 2; i++)
        {
            var question = new Question
            {
                QuizId = publishedQuiz.Id,
                Text = $"Question {i + 1}",
                DisplayOrder = i + 1
            };
            db.Questions.Add(question);
            await db.SaveChangesAsync();
            questionIds[i] = question.Id;

            var correct = new Answer { QuestionId = question.Id, Text = $"Correct {i + 1}", IsCorrect = true };
            var wrong = new Answer { QuestionId = question.Id, Text = $"Wrong {i + 1}", IsCorrect = false };
            db.Answers.AddRange(correct, wrong);
            await db.SaveChangesAsync();

            correctAnswerIds[i] = correct.Id;
            wrongAnswerIds[i] = wrong.Id;
        }

        return new SeedContext(user, publishedQuiz, draftQuiz, questionIds, correctAnswerIds, wrongAnswerIds);
    }
}

public sealed record SeedContext(
    ApplicationUser User,
    Quiz PublishedQuiz,
    Quiz DraftQuiz,
    int[] QuestionIds,
    int[] CorrectAnswerIds,
    int[] WrongAnswerIds);
