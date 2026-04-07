using QuizProject.Web.Models.ViewModels;

namespace QuizProject.Web.Services;

public interface ILeaderboardService
{
    Task<List<TopQuizViewModel>> GetTopQuizzesAsync(int count = 10);
    Task<List<TopUserViewModel>> GetTopUsersAsync(int count = 10);
}
