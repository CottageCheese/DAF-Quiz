using Microsoft.EntityFrameworkCore;
using QuizProject.Web.Models.Domain;
using QuizProject.Web.Models.ViewModels;
using QuizProject.Web.Repositories;

namespace QuizProject.Web.Services;

public class LeaderboardService : ILeaderboardService
{
    private readonly IRepository<QuizAttempt> _attempts;

    public LeaderboardService(IRepository<QuizAttempt> attempts)
    {
        _attempts = attempts;
    }

    public async Task<List<TopQuizViewModel>> GetTopQuizzesAsync(int count = 10)
    {
        var results = await _attempts.Query()
            .Where(a => a.CompletedAt != null)
            .GroupBy(a => new { a.QuizId, a.Quiz.Title })
            .Select(g => new
            {
                g.Key.QuizId,
                g.Key.Title,
                AttemptCount = g.Count()
            })
            .OrderByDescending(x => x.AttemptCount)
            .Take(count)
            .ToListAsync();

        return results
            .Select((r, i) => new TopQuizViewModel
            {
                Rank = i + 1,
                QuizTitle = r.Title,
                AttemptCount = r.AttemptCount
            })
            .ToList();
    }

    public async Task<List<TopUserViewModel>> GetTopUsersAsync(int count = 10)
    {
        var attempts = await _attempts.Query()
            .Where(a => a.CompletedAt != null && a.TotalQuestions > 0)
            .Select(a => new
            {
                a.UserId,
                a.User.Email,
                a.Score,
                a.TotalQuestions,
                QuizTitle = a.Quiz.Title
            })
            .ToListAsync();

        return attempts
            .GroupBy(a => new { a.UserId, a.Email })
            .Select(g =>
            {
                var best = g.OrderByDescending(a => (double)a.Score / a.TotalQuestions).First();
                return new { g.Key.Email, Best = best };
            })
            .OrderByDescending(x => (double)x.Best.Score / x.Best.TotalQuestions)
            .Take(count)
            .Select((x, i) => new TopUserViewModel
            {
                Rank = i + 1,
                UserName = x.Email ?? "Unknown",
                BestScorePercent = Math.Round((double)x.Best.Score / x.Best.TotalQuestions * 100, 1),
                QuizTitle = x.Best.QuizTitle
            })
            .ToList();
    }
}
