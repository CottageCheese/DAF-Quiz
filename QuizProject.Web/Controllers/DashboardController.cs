using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizProject.Web.Models.ViewModels;
using QuizProject.Web.Services;

namespace QuizProject.Web.Controllers;

[Authorize]
public class DashboardController(IPublicApiClient apiClient) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var historyTask = apiClient.GetMyHistoryAsync();
        var leaderboardTask = apiClient.GetLeaderboardAsync();
        await Task.WhenAll(historyTask, leaderboardTask);

        var history = historyTask.Result;
        var leaderboard = leaderboardTask.Result;

        var passed = history.Count(a => a.Percentage >= 50);
        var best = history.OrderByDescending(a => a.Percentage).FirstOrDefault();

        var model = new DashboardViewModel
        {
            UserName = User.Identity?.Name ?? string.Empty,
            TotalAttempts = history.Count,
            PassedCount = passed,
            PassRate = history.Count > 0 ? Math.Round((double)passed / history.Count * 100, 0) : 0,
            BestPercent = best?.Percentage ?? 0,
            BestQuizTitle = best?.QuizTitle,
            RecentAttempts = history
                .OrderByDescending(a => a.CompletedAt)
                .Take(5)
                .ToList(),
            TopScorers = leaderboard.TopUsers.Take(3).ToList()
        };

        return View(model);
    }
}
