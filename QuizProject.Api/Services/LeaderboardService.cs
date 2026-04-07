using Microsoft.EntityFrameworkCore;
using QuizProject.Api.Models.Domain;
using QuizProject.Api.Models.ViewModels;
using QuizProject.Api.Repositories;

namespace QuizProject.Api.Services;

public class LeaderboardService(IRepository<QuizAttempt> attempts) : ILeaderboardService
{
    public async Task<List<TopQuizViewModel>> GetTopQuizzesAsync(int count = 10, CancellationToken ct = default)
    {
        var results = await attempts.Query()
            .AsNoTracking()
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
            .ToListAsync(ct);

        return results
            .Select((r, i) => new TopQuizViewModel
            {
                Rank = i + 1,
                QuizTitle = r.Title,
                AttemptCount = r.AttemptCount
            })
            .ToList();
    }

    public async Task<List<TopUserViewModel>> GetTopUsersAsync(int count = 10, CancellationToken ct = default)
    {
        // Step 1: DB-level grouping to find top N users by best score percentage.
        var topUsers = await attempts.Query()
            .AsNoTracking()
            .Where(a => a.CompletedAt != null && a.TotalQuestions > 0)
            .GroupBy(a => new { a.UserId, a.User.DisplayName })
            .Select(g => new
            {
                g.Key.UserId,
                g.Key.DisplayName,
                BestScorePercent = g.Max(a => (double)a.Score / a.TotalQuestions * 100)
            })
            .OrderByDescending(u => u.BestScorePercent)
            .Take(count)
            .ToListAsync(ct);

        if (topUsers.Count == 0) return [];

        // Step 2: Fetch attempts for those users only to resolve the best quiz title.
        var topUserIds = topUsers.Select(u => u.UserId).ToList();
        var bestAttempts = await attempts.Query()
            .AsNoTracking()
            .Where(a => topUserIds.Contains(a.UserId) && a.CompletedAt != null && a.TotalQuestions > 0)
            .Select(a => new { a.UserId, a.Score, a.TotalQuestions, QuizTitle = a.Quiz.Title })
            .ToListAsync(ct);

        // Step 3: In-memory join (at most `count` users, bounded by the clamp in the controller).
        return topUsers
            .Select((u, i) =>
            {
                var best = bestAttempts
                    .Where(a => a.UserId == u.UserId)
                    .MaxBy(a => (double)a.Score / a.TotalQuestions);

                return new TopUserViewModel
                {
                    Rank = i + 1,
                    UserName = u.DisplayName,
                    BestScorePercent = Math.Round(u.BestScorePercent, 1),
                    QuizTitle = best?.QuizTitle ?? string.Empty
                };
            })
            .ToList();
    }
}
