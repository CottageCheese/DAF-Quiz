using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using QuizProject.Contracts;
using QuizProject.Domain.Data;
using QuizProject.Domain.Extensions;

namespace QuizProject.Domain.Services;

public class LeaderboardService(ApplicationDbContext db, IDistributedCache cache, ILogger<LeaderboardService> logger) : ILeaderboardService
{
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(12)
    };

    public async Task<List<TopQuizViewModel>> GetTopQuizzesAsync(int count = 10, CancellationToken ct = default)
    {
        var cacheKey = $"leaderboard:top-quizzes:{count}";
        var cached = await cache.GetAsync<List<TopQuizViewModel>>(cacheKey, ct);
        if (cached is not null) return cached;

        logger.LogDebug("Cache miss for top-quizzes (count={Count}) — querying database", count);

        var results = await db.QuizAttempts
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

        var topQuizzes = results
            .Select((r, i) => new TopQuizViewModel
            {
                Rank = i + 1,
                QuizTitle = r.Title,
                AttemptCount = r.AttemptCount
            })
            .ToList();

        await cache.SetAsync(cacheKey, topQuizzes, CacheOptions, ct);
        return topQuizzes;
    }

    public async Task<List<TopUserViewModel>> GetTopUsersAsync(int count = 10, CancellationToken ct = default)
    {
        var cacheKey = $"leaderboard:top-users:{count}";
        var cached = await cache.GetAsync<List<TopUserViewModel>>(cacheKey, ct);
        if (cached is not null) return cached;

        logger.LogDebug("Cache miss for top-users (count={Count}) — querying database", count);

        // Step 1: DB-level grouping to find top N users by best score percentage.
        var topUsers = await db.QuizAttempts
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

        if (topUsers.Count == 0)
        {
            await cache.SetAsync(cacheKey, new List<TopUserViewModel>(), CacheOptions, ct);
            return [];
        }

        // Step 2: Fetch attempts for those users only to resolve the best quiz title.
        var topUserIds = topUsers.Select(u => u.UserId).ToList();
        var bestAttempts = await db.QuizAttempts
            .AsNoTracking()
            .Where(a => topUserIds.Contains(a.UserId) && a.CompletedAt != null && a.TotalQuestions > 0)
            .Select(a => new { a.UserId, a.Score, a.TotalQuestions, QuizTitle = a.Quiz.Title })
            .ToListAsync(ct);

        // Step 3: In-memory join (at most `count` users, bounded by the clamp in the controller).
        var topUserResult = topUsers
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

        await cache.SetAsync(cacheKey, topUserResult, CacheOptions, ct);
        return topUserResult;
    }
}
