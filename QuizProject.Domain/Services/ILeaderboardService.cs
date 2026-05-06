using QuizProject.Contracts;

namespace QuizProject.Domain.Services;

public interface ILeaderboardService
{
    Task<List<TopQuizViewModel>> GetTopQuizzesAsync(int count = 10, CancellationToken ct = default);
    Task<List<TopUserViewModel>> GetTopUsersAsync(int count = 10, CancellationToken ct = default);
}
