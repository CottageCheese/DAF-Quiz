using QuizProject.Contracts;
using QuizProject.Web.Common.Services;

namespace QuizProject.Web.Admin.Services;

/// <summary>API client for the admin site.</summary>
public interface IAdminApiClient : IAuthApiClient
{
    Task<PagedResult<AdminQuizListViewModel>> GetAdminQuizzesAsync(int page = 1, int pageSize = 20);
    Task<AdminQuizDetailViewModel?> GetAdminQuizAsync(int id);
    Task<AdminQuizDetailViewModel?> CreateQuizAsync(string title, string? description);
    Task<AdminQuizDetailViewModel?> UpdateQuizAsync(int id, string title, string? description, DateTime? publishedAt);
    Task<bool> DeleteQuizAsync(int id);

    Task<AdminQuestionViewModel?> AddQuestionAsync(int quizId, string text, int displayOrder,
        List<(string Text, bool IsCorrect)> answers);

    Task<AdminQuestionViewModel?> UpdateQuestionAsync(int quizId, int questionId, string text, int displayOrder,
        List<(string Text, bool IsCorrect)> answers);

    Task<bool> DeleteQuestionAsync(int quizId, int questionId);
}
