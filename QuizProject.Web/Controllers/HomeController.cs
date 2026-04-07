using Microsoft.AspNetCore.Mvc;
using QuizProject.Web.Models.ViewModels;
using QuizProject.Web.Services;

namespace QuizProject.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILeaderboardService _leaderboardService;

    public HomeController(ILeaderboardService leaderboardService)
    {
        _leaderboardService = leaderboardService;
    }

    public async Task<IActionResult> Index()
    {
        var model = new LeaderboardViewModel
        {
            TopQuizzes = await _leaderboardService.GetTopQuizzesAsync(),
            TopUsers = await _leaderboardService.GetTopUsersAsync()
        };
        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
}

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
