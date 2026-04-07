using Microsoft.AspNetCore.Mvc;
using QuizProject.Api.Models.ViewModels;
using QuizProject.Api.Services;

namespace QuizProject.Api.Controllers;

[ApiController]
[Route("api/leaderboard")]
public class LeaderboardController(ILeaderboardService leaderboardService) : ControllerBase
{
    /// <summary>Returns the top most attempted quizzes.</summary>
    [HttpGet("top-quizzes")]
    [ProducesResponseType(typeof(List<TopQuizViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopQuizzes([FromQuery] int count = 10, CancellationToken ct = default)
    {
        count = Math.Clamp(count, 1, 50);
        var result = await leaderboardService.GetTopQuizzesAsync(count, ct);
        return Ok(result);
    }

    /// <summary>Returns the top users ranked by their best quiz score percentage.</summary>
    [HttpGet("top-users")]
    [ProducesResponseType(typeof(List<TopUserViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopUsers([FromQuery] int count = 10, CancellationToken ct = default)
    {
        count = Math.Clamp(count, 1, 50);
        var result = await leaderboardService.GetTopUsersAsync(count, ct);
        return Ok(result);
    }
}
