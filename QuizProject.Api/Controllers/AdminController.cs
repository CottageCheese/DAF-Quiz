using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizProject.Api.Models.ViewModels;
using QuizProject.Api.Services;

namespace QuizProject.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class AdminController(IAdminQuizService adminService) : ControllerBase
{
    // ── Quizzes ───────────────────────────────────────────────────────────────

    /// <summary>Returns all quizzes (published and drafts).</summary>
    [HttpGet("quizzes")]
    [ProducesResponseType(typeof(List<AdminQuizListViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQuizzes(CancellationToken ct)
    {
        var quizzes = await adminService.GetAllQuizzesAsync(ct);
        return Ok(quizzes);
    }

    /// <summary>Returns a single quiz with all questions and answers.</summary>
    [HttpGet("quizzes/{id:int}")]
    [ProducesResponseType(typeof(AdminQuizDetailViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetQuiz(int id, CancellationToken ct)
    {
        var quiz = await adminService.GetQuizDetailAsync(id, ct);
        if (quiz is null) return NotFound();
        return Ok(quiz);
    }

    /// <summary>Creates a new quiz template.</summary>
    [HttpPost("quizzes")]
    [ProducesResponseType(typeof(AdminQuizDetailViewModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateQuiz([FromBody] CreateQuizRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var userEmail = User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue("email") ?? string.Empty;

        var quiz = await adminService.CreateQuizAsync(request, userId, userEmail, ct);
        return CreatedAtAction(nameof(GetQuiz), new { id = quiz.Id }, quiz);
    }

    /// <summary>Updates quiz metadata and/or publish date.</summary>
    [HttpPut("quizzes/{id:int}")]
    [ProducesResponseType(typeof(AdminQuizDetailViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateQuiz(int id, [FromBody] UpdateQuizRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var quiz = await adminService.UpdateQuizAsync(id, request, ct);
        if (quiz is null) return NotFound();
        return Ok(quiz);
    }

    /// <summary>Deletes a quiz and all its questions/answers.</summary>
    [HttpDelete("quizzes/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteQuiz(int id, CancellationToken ct)
    {
        var deleted = await adminService.DeleteQuizAsync(id, ct);
        if (!deleted) return NotFound();
        return NoContent();
    }

    // ── Questions ─────────────────────────────────────────────────────────────

    /// <summary>Adds a question (with answers) to a quiz.</summary>
    [HttpPost("quizzes/{quizId:int}/questions")]
    [ProducesResponseType(typeof(AdminQuestionViewModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddQuestion(int quizId, [FromBody] UpsertQuestionRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (!request.Answers.Any(a => a.IsCorrect))
            return BadRequest(new { message = "At least one answer must be marked as correct." });

        var question = await adminService.AddQuestionAsync(quizId, request, ct);
        return StatusCode(StatusCodes.Status201Created, question);
    }

    /// <summary>Updates a question and replaces all its answers.</summary>
    [HttpPut("quizzes/{quizId:int}/questions/{questionId:int}")]
    [ProducesResponseType(typeof(AdminQuestionViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateQuestion(int quizId, int questionId, [FromBody] UpsertQuestionRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (!request.Answers.Any(a => a.IsCorrect))
            return BadRequest(new { message = "At least one answer must be marked as correct." });

        var question = await adminService.UpdateQuestionAsync(questionId, request, ct);
        if (question is null) return NotFound();
        return Ok(question);
    }

    /// <summary>Deletes a question and its answers.</summary>
    [HttpDelete("quizzes/{quizId:int}/questions/{questionId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteQuestion(int quizId, int questionId, CancellationToken ct)
    {
        var deleted = await adminService.DeleteQuestionAsync(questionId, ct);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
