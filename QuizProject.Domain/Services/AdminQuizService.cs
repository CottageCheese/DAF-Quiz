using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QuizProject.Contracts;
using QuizProject.Domain.Models.Domain;
using QuizProject.Domain.Repositories;

namespace QuizProject.Domain.Services;

public class AdminQuizService(
    IRepository<Quiz> quizzes,
    IRepository<Question> questions,
    IRepository<Answer> answers,
    IRepository<QuizAttempt> attempts,
    IRepository<QuizAttemptAnswer> attemptAnswers,
    IMemoryCache cache) : IAdminQuizService
{
    public async Task<List<AdminQuizListViewModel>> GetAllQuizzesAsync(CancellationToken ct = default)
    {
        return await quizzes.Query()
            .AsNoTracking()
            .Select(q => new AdminQuizListViewModel
            {
                Id = q.Id,
                Title = q.Title,
                Description = q.Description,
                CreatedAt = q.CreatedAt,
                CreatedByEmail = q.CreatedByEmail,
                PublishedAt = q.PublishedAt,
                IsPublished = q.PublishedAt != null && q.PublishedAt <= DateTime.UtcNow,
                QuestionCount = q.Questions.Count
            })
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<AdminQuizDetailViewModel?> GetQuizDetailAsync(int quizId, CancellationToken ct = default)
    {
        var quiz = await quizzes.Query()
            .AsNoTracking()
            .Include(q => q.Questions.OrderBy(qu => qu.DisplayOrder))
            .ThenInclude(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == quizId, ct);

        if (quiz is null) return null;

        return MapToDetail(quiz);
    }

    public async Task<AdminQuizDetailViewModel> CreateQuizAsync(
        CreateQuizRequest request, string userId, string userEmail, CancellationToken ct = default)
    {
        var quiz = new Quiz
        {
            Title = request.Title,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = userId,
            CreatedByEmail = userEmail
        };

        await quizzes.AddAsync(quiz);
        await quizzes.SaveChangesAsync();

        return MapToDetail(quiz);
    }

    public async Task<AdminQuizDetailViewModel?> UpdateQuizAsync(int quizId, UpdateQuizRequest request, CancellationToken ct = default)
    {
        var quiz = await quizzes.Query()
            .Include(q => q.Questions.OrderBy(qu => qu.DisplayOrder))
            .ThenInclude(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == quizId, ct);

        if (quiz is null) return null;

        quiz.Title = request.Title;
        quiz.Description = request.Description;
        quiz.PublishedAt = request.PublishedAt;

        quizzes.Update(quiz);
        await quizzes.SaveChangesAsync();

        cache.Remove(QuizService.ActiveQuizzesCacheKey);

        return MapToDetail(quiz);
    }

    public async Task<bool> DeleteQuizAsync(int quizId, CancellationToken ct = default)
    {
        var quiz = await quizzes.Query()
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.Id == quizId, ct);
        if (quiz is null) return false;

        var questionIds = quiz.Questions.Select(q => q.Id).ToList();
        var relatedAttemptAnswers = await attemptAnswers.Query()
            .Where(aa => questionIds.Contains(aa.QuestionId))
            .ToListAsync(ct);
        foreach (var aa in relatedAttemptAnswers)
            attemptAnswers.Remove(aa);

        var relatedAttempts = await attempts.Query()
            .Where(a => a.QuizId == quizId)
            .ToListAsync(ct);
        foreach (var attempt in relatedAttempts)
            attempts.Remove(attempt);

        quizzes.Remove(quiz);
        await quizzes.SaveChangesAsync();

        cache.Remove(QuizService.ActiveQuizzesCacheKey);
        return true;
    }

    public async Task<AdminQuestionViewModel> AddQuestionAsync(int quizId, UpsertQuestionRequest request, CancellationToken ct = default)
    {
        var question = new Question
        {
            QuizId = quizId,
            Text = request.Text,
            DisplayOrder = request.DisplayOrder,
            Answers = request.Answers.Select(a => new Answer
            {
                Text = a.Text,
                IsCorrect = a.IsCorrect
            }).ToList()
        };

        await questions.AddAsync(question);
        await questions.SaveChangesAsync();

        cache.Remove(QuizService.ActiveQuizzesCacheKey);

        return MapToQuestion(question);
    }

    public async Task<AdminQuestionViewModel?> UpdateQuestionAsync(int questionId, UpsertQuestionRequest request, CancellationToken ct = default)
    {
        var question = await questions.Query()
            .Include(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == questionId, ct);

        if (question is null) return null;

        question.Text = request.Text;
        question.DisplayOrder = request.DisplayOrder;

        // Remove attempt answers referencing this question before replacing answers
        var relatedAttemptAnswers = await attemptAnswers.Query()
            .Where(aa => aa.QuestionId == questionId)
            .ToListAsync(ct);
        foreach (var aa in relatedAttemptAnswers)
            attemptAnswers.Remove(aa);

        // Remove old answers and replace with new set
        foreach (var existing in question.Answers.ToList())
            answers.Remove(existing);

        question.Answers = request.Answers.Select(a => new Answer
        {
            QuestionId = questionId,
            Text = a.Text,
            IsCorrect = a.IsCorrect
        }).ToList();

        questions.Update(question);
        await questions.SaveChangesAsync();

        cache.Remove(QuizService.ActiveQuizzesCacheKey);

        return MapToQuestion(question);
    }

    public async Task<bool> DeleteQuestionAsync(int questionId, CancellationToken ct = default)
    {
        var question = await questions.Query().FirstOrDefaultAsync(q => q.Id == questionId, ct);
        if (question is null) return false;

        var relatedAttemptAnswers = await attemptAnswers.Query()
            .Where(aa => aa.QuestionId == questionId)
            .ToListAsync(ct);
        foreach (var aa in relatedAttemptAnswers)
            attemptAnswers.Remove(aa);

        questions.Remove(question);
        await questions.SaveChangesAsync();

        cache.Remove(QuizService.ActiveQuizzesCacheKey);
        return true;
    }

    private static AdminQuizDetailViewModel MapToDetail(Quiz quiz) => new()
    {
        Id = quiz.Id,
        Title = quiz.Title,
        Description = quiz.Description,
        CreatedAt = quiz.CreatedAt,
        CreatedByEmail = quiz.CreatedByEmail,
        PublishedAt = quiz.PublishedAt,
        IsPublished = quiz.PublishedAt != null && quiz.PublishedAt <= DateTime.UtcNow,
        Questions = quiz.Questions
            .OrderBy(q => q.DisplayOrder)
            .Select(MapToQuestion)
            .ToList()
    };

    private static AdminQuestionViewModel MapToQuestion(Question q) => new()
    {
        Id = q.Id,
        Text = q.Text,
        DisplayOrder = q.DisplayOrder,
        Answers = q.Answers.Select(a => new AdminAnswerViewModel
        {
            Id = a.Id,
            Text = a.Text,
            IsCorrect = a.IsCorrect
        }).ToList()
    };
}
