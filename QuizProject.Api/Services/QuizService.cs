using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QuizProject.Api.Models.Domain;
using QuizProject.Api.Models.ViewModels;
using QuizProject.Api.Repositories;

namespace QuizProject.Api.Services;

public class QuizService(
    IRepository<Quiz> quizzes,
    IRepository<Question> questions,
    IRepository<QuizAttempt> attempts,
    IRepository<QuizAttemptAnswer> attemptAnswers,
    IMemoryCache cache)
    : IQuizService
{
    internal const string ActiveQuizzesCacheKey = "quizzes:active";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public async Task<List<QuizListViewModel>> GetActiveQuizzesAsync(CancellationToken ct = default)
    {
        if (cache.TryGetValue(ActiveQuizzesCacheKey, out List<QuizListViewModel>? cached))
            return cached!;

        var now = DateTime.UtcNow;
        var result = await quizzes.Query()
            .AsNoTracking()
            .Where(q => q.PublishedAt != null && q.PublishedAt <= now)
            .Select(q => new QuizListViewModel
            {
                Id = q.Id,
                Title = q.Title,
                Description = q.Description,
                QuestionCount = q.Questions.Count,
                AttemptCount = q.Attempts.Count(a => a.CompletedAt != null),
                CreatedAt = q.CreatedAt
            })
            .OrderBy(q => q.Title)
            .ToListAsync(ct);

        cache.Set(ActiveQuizzesCacheKey, result, CacheDuration);
        return result;
    }

    public async Task<TakeQuizViewModel?> StartAttemptAsync(int quizId, string userId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var quiz = await quizzes.Query()
            .AsNoTracking()
            .Where(q => q.Id == quizId && q.PublishedAt != null && q.PublishedAt <= now)
            .Include(q => q.Questions.OrderBy(qu => qu.DisplayOrder))
            .ThenInclude(q => q.Answers)
            .FirstOrDefaultAsync(ct);

        if (quiz is null) return null;

        var attempt = new QuizAttempt
        {
            UserId = userId,
            QuizId = quizId,
            StartedAt = DateTime.UtcNow,
            TotalQuestions = quiz.Questions.Count
        };

        await attempts.AddAsync(attempt);
        await attempts.SaveChangesAsync();

        return new TakeQuizViewModel
        {
            AttemptId = attempt.Id,
            QuizId = quiz.Id,
            QuizTitle = quiz.Title,
            TotalQuestions = quiz.Questions.Count,
            Questions = quiz.Questions
                .OrderBy(q => q.DisplayOrder)
                .Select(q => new QuizQuestionViewModel
                {
                    QuestionId = q.Id,
                    Text = q.Text,
                    DisplayOrder = q.DisplayOrder,
                    Answers = q.Answers
                        .OrderBy(_ => Random.Shared.Next())
                        .Select(a => new QuizAnswerViewModel
                        {
                            AnswerId = a.Id,
                            Text = a.Text
                        })
                        .ToList()
                })
                .ToList()
        };
    }

    public async Task<QuizResultViewModel?> SubmitAttemptAsync(SubmitQuizViewModel submission, string userId, CancellationToken ct = default)
    {
        var attempt = await attempts.Query()
            .Where(a => a.Id == submission.AttemptId && a.UserId == userId && a.CompletedAt == null)
            .Include(a => a.Quiz)
            .FirstOrDefaultAsync(ct);

        if (attempt is null) return null;

        var questionIds = submission.Selections.Select(s => s.QuestionId).ToList();
        var questionList = await questions.Query()
            .AsNoTracking()
            .Where(q => questionIds.Contains(q.Id) && q.QuizId == attempt.QuizId)
            .Include(q => q.Answers)
            .ToListAsync(ct);

        var attemptAnswerList = new List<QuizAttemptAnswer>();
        var score = 0;

        foreach (var selection in submission.Selections)
        {
            var question = questionList.FirstOrDefault(q => q.Id == selection.QuestionId);
            if (question is null) continue;

            var selectedAnswer = question.Answers.FirstOrDefault(a => a.Id == selection.SelectedAnswerId);
            if (selectedAnswer is null) continue;

            var isCorrect = selectedAnswer.IsCorrect;
            if (isCorrect) score++;

            attemptAnswerList.Add(new QuizAttemptAnswer
            {
                AttemptId = attempt.Id,
                QuestionId = selection.QuestionId,
                SelectedAnswerId = selection.SelectedAnswerId,
                IsCorrect = isCorrect
            });
        }

        attempt.Score = score;
        attempt.CompletedAt = DateTime.UtcNow;
        await attemptAnswers.AddRangeAsync(attemptAnswerList);
        await attemptAnswers.SaveChangesAsync();

        return await BuildResultViewModelAsync(attempt.Id, ct);
    }

    public async Task<QuizResultViewModel?> GetResultAsync(int attemptId, string userId, CancellationToken ct = default)
    {
        var exists = await attempts.Query()
            .AnyAsync(a => a.Id == attemptId && a.UserId == userId && a.CompletedAt != null, ct);

        if (!exists) return null;

        return await BuildResultViewModelAsync(attemptId, ct);
    }

    private async Task<QuizResultViewModel> BuildResultViewModelAsync(int attemptId, CancellationToken ct)
    {
        // Single query — no N+1. Question.Answers loaded so correct answer is resolved in memory.
        var attempt = await attempts.Query()
            .AsNoTracking()
            .Include(a => a.Quiz)
            .Include(a => a.AttemptAnswers)
                .ThenInclude(aa => aa.Question)
                    .ThenInclude(q => q.Answers)
            .Include(a => a.AttemptAnswers)
                .ThenInclude(aa => aa.SelectedAnswer)
            .FirstAsync(a => a.Id == attemptId, ct);

        var answerDetails = attempt.AttemptAnswers
            .OrderBy(aa => aa.Question.DisplayOrder)
            .Select(aa => new ResultAnswerViewModel
            {
                QuestionText = aa.Question.Text,
                SelectedAnswerText = aa.SelectedAnswer.Text,
                CorrectAnswerText = aa.Question.Answers.FirstOrDefault(a => a.IsCorrect)?.Text ?? string.Empty,
                IsCorrect = aa.IsCorrect
            })
            .ToList();

        return new QuizResultViewModel
        {
            AttemptId = attempt.Id,
            QuizTitle = attempt.Quiz.Title,
            Score = attempt.Score,
            TotalQuestions = attempt.TotalQuestions,
            Answers = answerDetails
        };
    }
}
