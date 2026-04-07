using QuizProject.Web.Models.ViewModels;

namespace QuizProject.Web.Services;

public interface IApiClient
{
    // Auth
    Task<AuthTokens?> LoginAsync(string email, string password);
    Task<AuthTokens?> RegisterAsync(string email, string password);
    Task RevokeTokenAsync(string refreshToken);

    // Quizzes
    Task<List<QuizListViewModel>> GetQuizzesAsync();
    Task<TakeQuizViewModel?> StartAttemptAsync(int quizId);
    Task<QuizResultViewModel?> SubmitAttemptAsync(int attemptId, List<QuestionAnswerSelection> selections);
    Task<QuizResultViewModel?> GetResultAsync(int attemptId);

    // Leaderboard
    Task<LeaderboardViewModel> GetLeaderboardAsync();
}

/// <summary>Tokens returned by the API auth endpoints.</summary>
public sealed record AuthTokens(string AccessToken, string RefreshToken, int ExpiresIn);
