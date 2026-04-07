using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizProject.Web.Models.ViewModels;
using QuizProject.Web.Services;

namespace QuizProject.Web.Controllers.Api;

[ApiController]
[Route("api/quizzes")]
[Authorize]
public class QuizzesApiController(IQuizService quizService) : ControllerBase
{
    /// <summary>Returns all active quizzes.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<QuizListViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetQuizzes()
    {
        var quizzes = await quizService.GetActiveQuizzesAsync();
        return Ok(quizzes);
    }

    /// <summary>Starts a quiz attempt and returns questions with shuffled answers.</summary>
    [HttpPost("{quizId:int}/start")]
    [ProducesResponseType(typeof(TakeQuizViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> StartAttempt(int quizId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        var model = await quizService.StartAttemptAsync(quizId, userId);
        if (model is null) return NotFound(new { message = "Quiz not found or inactive." });

        return Ok(model);
    }

    /// <summary>Submits answers for an attempt and returns the result.</summary>
    [HttpPost("attempts/{attemptId:int}/submit")]
    [ProducesResponseType(typeof(QuizResultViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SubmitAttempt(int attemptId, [FromBody] List<QuestionAnswerSelection> selections)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        var submission = new SubmitQuizViewModel { AttemptId = attemptId, Selections = selections };
        var result = await quizService.SubmitAttemptAsync(submission, userId);

        if (result is null) return NotFound(new { message = "Attempt not found or already completed." });

        return Ok(result);
    }

    /// <summary>Returns the result of a previously completed attempt.</summary>
    [HttpGet("attempts/{attemptId:int}/result")]
    [ProducesResponseType(typeof(QuizResultViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetResult(int attemptId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        var result = await quizService.GetResultAsync(attemptId, userId);
        if (result is null) return NotFound(new { message = "Result not found." });

        return Ok(result);
    }
}