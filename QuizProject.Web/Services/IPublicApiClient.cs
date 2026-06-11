using QuizProject.Contracts;
using QuizProject.Web.Models.ViewModels;
using QuizProject.Web.Common.Services;

namespace QuizProject.Web.Services;

/// <summary>API client for the public quiz-taking site.</summary>
public interface IPublicApiClient : IAuthApiClient
{
    // Quizzes
    Task<List<QuizListViewModel>> GetQuizzesAsync();
    Task<TakeQuizViewModel?> StartAttemptAsync(int quizId);
    Task<QuizResultViewModel?> SubmitAttemptAsync(int attemptId, List<QuestionAnswerSelection> selections);
    Task<QuizResultViewModel?> GetResultAsync(int attemptId);

    // Leaderboard
    Task<LeaderboardViewModel> GetLeaderboardAsync();
}
