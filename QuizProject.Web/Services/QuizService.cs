using Microsoft.EntityFrameworkCore;
using QuizProject.Web.Data;
using QuizProject.Web.Models.Domain;
using QuizProject.Web.Models.ViewModels;

namespace QuizProject.Web.Services;

public class QuizService : IQuizService
{
    private readonly ApplicationDbContext _db;

    public QuizService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<QuizListViewModel>> GetActiveQuizzesAsync()
    {
        return await _db.Quizzes
            .Where(q => q.IsActive)
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
            .ToListAsync();
    }

    public async Task<TakeQuizViewModel?> StartAttemptAsync(int quizId, string userId)
    {
        var quiz = await _db.Quizzes
            .Where(q => q.Id == quizId && q.IsActive)
            .Include(q => q.Questions.OrderBy(qu => qu.DisplayOrder))
                .ThenInclude(q => q.Answers)
            .FirstOrDefaultAsync();

        if (quiz is null) return null;

        var attempt = new QuizAttempt
        {
            UserId = userId,
            QuizId = quizId,
            StartedAt = DateTime.UtcNow,
            TotalQuestions = quiz.Questions.Count
        };

        _db.QuizAttempts.Add(attempt);
        await _db.SaveChangesAsync();

        var rng = new Random();

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
                        .OrderBy(_ => rng.Next())   // shuffle answers
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

    public async Task<QuizResultViewModel?> SubmitAttemptAsync(SubmitQuizViewModel submission, string userId)
    {
        var attempt = await _db.QuizAttempts
            .Where(a => a.Id == submission.AttemptId && a.UserId == userId && a.CompletedAt == null)
            .Include(a => a.Quiz)
            .FirstOrDefaultAsync();

        if (attempt is null) return null;

        var questionIds = submission.Selections.Select(s => s.QuestionId).ToList();
        var questions = await _db.Questions
            .Where(q => questionIds.Contains(q.Id) && q.QuizId == attempt.QuizId)
            .Include(q => q.Answers)
            .ToListAsync();

        var attemptAnswers = new List<QuizAttemptAnswer>();
        int score = 0;

        foreach (var selection in submission.Selections)
        {
            var question = questions.FirstOrDefault(q => q.Id == selection.QuestionId);
            if (question is null) continue;

            var selectedAnswer = question.Answers.FirstOrDefault(a => a.Id == selection.SelectedAnswerId);
            if (selectedAnswer is null) continue;

            bool isCorrect = selectedAnswer.IsCorrect;
            if (isCorrect) score++;

            attemptAnswers.Add(new QuizAttemptAnswer
            {
                AttemptId = attempt.Id,
                QuestionId = selection.QuestionId,
                SelectedAnswerId = selection.SelectedAnswerId,
                IsCorrect = isCorrect
            });
        }

        attempt.Score = score;
        attempt.CompletedAt = DateTime.UtcNow;
        _db.QuizAttemptAnswers.AddRange(attemptAnswers);
        await _db.SaveChangesAsync();

        return await BuildResultViewModelAsync(attempt.Id);
    }

    public async Task<QuizResultViewModel?> GetResultAsync(int attemptId, string userId)
    {
        var attempt = await _db.QuizAttempts
            .Where(a => a.Id == attemptId && a.UserId == userId && a.CompletedAt != null)
            .FirstOrDefaultAsync();

        if (attempt is null) return null;

        return await BuildResultViewModelAsync(attemptId);
    }

    private async Task<QuizResultViewModel> BuildResultViewModelAsync(int attemptId)
    {
        var attempt = await _db.QuizAttempts
            .Include(a => a.Quiz)
            .Include(a => a.AttemptAnswers)
                .ThenInclude(aa => aa.Question)
            .Include(a => a.AttemptAnswers)
                .ThenInclude(aa => aa.SelectedAnswer)
            .FirstAsync(a => a.Id == attemptId);

        var answerDetails = new List<ResultAnswerViewModel>();
        foreach (var aa in attempt.AttemptAnswers.OrderBy(a => a.Question.DisplayOrder))
        {
            var correctAnswer = await _db.Answers
                .Where(a => a.QuestionId == aa.QuestionId && a.IsCorrect)
                .Select(a => a.Text)
                .FirstOrDefaultAsync() ?? string.Empty;

            answerDetails.Add(new ResultAnswerViewModel
            {
                QuestionText = aa.Question.Text,
                SelectedAnswerText = aa.SelectedAnswer.Text,
                CorrectAnswerText = correctAnswer,
                IsCorrect = aa.IsCorrect
            });
        }

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
