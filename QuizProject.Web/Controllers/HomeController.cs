using Microsoft.AspNetCore.Mvc;
using QuizProject.Web.Models.ViewModels;
using QuizProject.Web.Services;

namespace QuizProject.Web.Controllers;

public class HomeController(ILeaderboardService leaderboardService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var model = new LeaderboardViewModel
        {
            TopQuizzes = await leaderboardService.GetTopQuizzesAsync(),
            TopUsers = await leaderboardService.GetTopUsersAsync()
        };
        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
    }
}