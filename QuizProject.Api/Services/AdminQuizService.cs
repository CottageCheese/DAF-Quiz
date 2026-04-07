using Microsoft.EntityFrameworkCore;
using QuizProject.Api.Models.Domain;
using QuizProject.Api.Models.ViewModels;
using QuizProject.Api.Repositories;

namespace QuizProject.Api.Services;

public class AdminQuizService(
    IRepository<Quiz> quizzes,
    IRepository<Question> questions,
    IRepository<Answer> answers) : IAdminQuizService
{
    public async Task<List<AdminQuizListViewModel>> GetAllQuizzesAsync()
    {
        return await quizzes.Query()
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
            .ToListAsync();
    }

    public async Task<AdminQuizDetailViewModel?> GetQuizDetailAsync(int quizId)
    {
        var quiz = await quizzes.Query()
            .Include(q => q.Questions.OrderBy(qu => qu.DisplayOrder))
            .ThenInclude(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == quizId);

        if (quiz is null) return null;

        return MapToDetail(quiz);
    }

    public async Task<AdminQuizDetailViewModel> CreateQuizAsync(
        CreateQuizRequest request, string userId, string userEmail)
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

    public async Task<AdminQuizDetailViewModel?> UpdateQuizAsync(int quizId, UpdateQuizRequest request)
    {
        var quiz = await quizzes.Query()
            .Include(q => q.Questions.OrderBy(qu => qu.DisplayOrder))
            .ThenInclude(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == quizId);

        if (quiz is null) return null;

        quiz.Title = request.Title;
        quiz.Description = request.Description;
        quiz.PublishedAt = request.PublishedAt;

        quizzes.Update(quiz);
        await quizzes.SaveChangesAsync();

        return MapToDetail(quiz);
    }

    public async Task<bool> DeleteQuizAsync(int quizId)
    {
        var quiz = await quizzes.Query().FirstOrDefaultAsync(q => q.Id == quizId);
        if (quiz is null) return false;

        quizzes.Remove(quiz);
        await quizzes.SaveChangesAsync();
        return true;
    }

    public async Task<AdminQuestionViewModel> AddQuestionAsync(int quizId, UpsertQuestionRequest request)
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

        return MapToQuestion(question);
    }

    public async Task<AdminQuestionViewModel?> UpdateQuestionAsync(int questionId, UpsertQuestionRequest request)
    {
        var question = await questions.Query()
            .Include(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == questionId);

        if (question is null) return null;

        question.Text = request.Text;
        question.DisplayOrder = request.DisplayOrder;

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

        return MapToQuestion(question);
    }

    public async Task<bool> DeleteQuestionAsync(int questionId)
    {
        var question = await questions.Query().FirstOrDefaultAsync(q => q.Id == questionId);
        if (question is null) return false;

        questions.Remove(question);
        await questions.SaveChangesAsync();
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
