using QuizProject.Contracts;
using QuizProject.Web.Models.ViewModels;

namespace QuizProject.Web.Services;

public interface IApiClient
{
    // Auth
    Task<ApiResult<AuthTokens>> LoginAsync(string email, string password);
    Task<ApiResult<AuthTokens>> RegisterAsync(string email, string password, string displayName);
    Task RevokeTokenAsync(string refreshToken);

    // Quizzes
    Task<List<QuizListViewModel>> GetQuizzesAsync();
    Task<TakeQuizViewModel?> StartAttemptAsync(int quizId);
    Task<QuizResultViewModel?> SubmitAttemptAsync(int attemptId, List<QuestionAnswerSelection> selections);
    Task<QuizResultViewModel?> GetResultAsync(int attemptId);

    // Leaderboard
    Task<LeaderboardViewModel> GetLeaderboardAsync();

    // Admin
    Task<List<AdminQuizListViewModel>> GetAdminQuizzesAsync();
    Task<AdminQuizDetailViewModel?> GetAdminQuizAsync(int id);
    Task<AdminQuizDetailViewModel?> CreateQuizAsync(string title, string? description);
    Task<AdminQuizDetailViewModel?> UpdateQuizAsync(int id, string title, string? description, DateTime? publishedAt);
    Task<bool> DeleteQuizAsync(int id);
    Task<AdminQuestionViewModel?> AddQuestionAsync(int quizId, string text, int displayOrder, List<(string Text, bool IsCorrect)> answers);
    Task<AdminQuestionViewModel?> UpdateQuestionAsync(int quizId, int questionId, string text, int displayOrder, List<(string Text, bool IsCorrect)> answers);
    Task<bool> DeleteQuestionAsync(int quizId, int questionId);
}