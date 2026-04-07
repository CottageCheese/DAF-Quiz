using Microsoft.AspNetCore.Mvc;
using QuizProject.Web.Models.ViewModels;
using QuizProject.Web.Services;

namespace QuizProject.Web.Controllers.Api;

[ApiController]
[Route("api/leaderboard")]
public class LeaderboardApiController : ControllerBase
{
    private readonly ILeaderboardService _leaderboardService;

    public LeaderboardApiController(ILeaderboardService leaderboardService)
    {
        _leaderboardService = leaderboardService;
    }

    /// <summary>Returns the top 10 most attempted quizzes.</summary>
    [HttpGet("top-quizzes")]
    [ProducesResponseType(typeof(List<TopQuizViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopQuizzes([FromQuery] int count = 10)
    {
        count = Math.Clamp(count, 1, 50);
        var result = await _leaderboardService.GetTopQuizzesAsync(count);
        return Ok(result);
    }

    /// <summary>Returns the top 10 users ranked by their best quiz score percentage.</summary>
    [HttpGet("top-users")]
    [ProducesResponseType(typeof(List<TopUserViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopUsers([FromQuery] int count = 10)
    {
        count = Math.Clamp(count, 1, 50);
        var result = await _leaderboardService.GetTopUsersAsync(count);
        return Ok(result);
    }
}
