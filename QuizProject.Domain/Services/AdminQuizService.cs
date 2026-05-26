using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using QuizProject.Contracts;
using QuizProject.Domain.Data;
using QuizProject.Domain.Exceptions;
using QuizProject.Domain.Models.Domain;

namespace QuizProject.Domain.Services;

public class AdminQuizService(ApplicationDbContext db, IDistributedCache cache, ILogger<AdminQuizService> logger) : IAdminQuizService
{
    public async Task<PagedResult<AdminQuizListViewModel>> GetAllQuizzesAsync(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var query = db.Quizzes.AsNoTracking();
        var total = await query.CountAsync(ct);

        var items = await query
            .Select(q => new AdminQuizListViewModel
            {
                Id = q.Id,
                Title = q.Title,
                Description = q.Description,
                CreatedAt = q.CreatedAt,
                CreatedByEmail = q.CreatedByEmail,
                PublishedAt = q.PublishedAt,
                IsPublished = q.PublishedAt != null && q.PublishedAt <= DateTime.UtcNow,
                QuestionCount = q.Questions.Count()
            })
            .OrderByDescending(q => q.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<AdminQuizListViewModel>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<AdminQuizDetailViewModel?> GetQuizDetailAsync(int quizId, CancellationToken ct = default)
    {
        var quiz = await db.Quizzes
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

        db.Add(quiz);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Quiz {QuizId} created by {UserEmail}", quiz.Id, userEmail);
        return MapToDetail(quiz);
    }

    public async Task<AdminQuizDetailViewModel?> UpdateQuizAsync(int quizId, UpdateQuizRequest request, CancellationToken ct = default)
    {
        var quiz = await db.Quizzes
            .Include(q => q.Questions.OrderBy(qu => qu.DisplayOrder))
            .ThenInclude(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == quizId, ct);

        if (quiz is null) return null;

        quiz.Title = request.Title;
        quiz.Description = request.Description;
        quiz.PublishedAt = request.PublishedAt;

        await db.SaveChangesAsync(ct);

        await cache.RemoveAsync(QuizService.ActiveQuizzesCacheKey);
        logger.LogInformation("Quiz {QuizId} updated", quizId);
        return MapToDetail(quiz);
    }

    public async Task<bool> DeleteQuizAsync(int quizId, CancellationToken ct = default)
    {
        var quiz = await db.Quizzes
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.Id == quizId, ct);
        if (quiz is null) return false;

        var questionIds = quiz.Questions.Select(q => q.Id).ToList();
        await db.QuizAttemptAnswers
            .Where(aa => questionIds.Contains(aa.QuestionId))
            .ExecuteDeleteAsync(ct);

        await db.QuizAttempts
            .Where(a => a.QuizId == quizId)
            .ExecuteDeleteAsync(ct);

        db.Remove(quiz);
        await db.SaveChangesAsync(ct);

        await cache.RemoveAsync(QuizService.ActiveQuizzesCacheKey);
        logger.LogInformation("Quiz {QuizId} deleted", quizId);
        return true;
    }

    public async Task<AdminQuestionViewModel> AddQuestionAsync(int quizId, UpsertQuestionRequest request, CancellationToken ct = default)
    {
        if (!request.Answers.Any(a => a.IsCorrect))
            throw new DomainValidationException("At least one answer must be marked as correct.");

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

        db.Add(question);
        await db.SaveChangesAsync(ct);

        await cache.RemoveAsync(QuizService.ActiveQuizzesCacheKey);
        logger.LogInformation("Question {QuestionId} added to quiz {QuizId}", question.Id, quizId);
        return MapToQuestion(question);
    }

    public async Task<AdminQuestionViewModel?> UpdateQuestionAsync(int questionId, UpsertQuestionRequest request, CancellationToken ct = default)
    {
        if (!request.Answers.Any(a => a.IsCorrect))
            throw new DomainValidationException("At least one answer must be marked as correct.");

        var question = await db.Questions
            .Include(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == questionId, ct);

        if (question is null) return null;

        question.Text = request.Text;
        question.DisplayOrder = request.DisplayOrder;

        // Remove attempt answers referencing this question before replacing answers
        await db.QuizAttemptAnswers
            .Where(aa => aa.QuestionId == questionId)
            .ExecuteDeleteAsync(ct);

        // Remove old answers and replace with new set
        db.RemoveRange(question.Answers);

        question.Answers = request.Answers.Select(a => new Answer
        {
            QuestionId = questionId,
            Text = a.Text,
            IsCorrect = a.IsCorrect
        }).ToList();

        await db.SaveChangesAsync(ct);

        await cache.RemoveAsync(QuizService.ActiveQuizzesCacheKey);
        logger.LogInformation("Question {QuestionId} updated", questionId);
        return MapToQuestion(question);
    }

    public async Task<bool> DeleteQuestionAsync(int questionId, CancellationToken ct = default)
    {
        var question = await db.Questions.FirstOrDefaultAsync(q => q.Id == questionId, ct);
        if (question is null) return false;

        await db.QuizAttemptAnswers
            .Where(aa => aa.QuestionId == questionId)
            .ExecuteDeleteAsync(ct);

        db.Remove(question);
        await db.SaveChangesAsync(ct);

        await cache.RemoveAsync(QuizService.ActiveQuizzesCacheKey);
        logger.LogInformation("Question {QuestionId} deleted", questionId);
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
        IsPublished = quiz.IsPublished,
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
