using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizProject.Web.Models.ViewModels;
using QuizProject.Web.Services;
using System.Security.Claims;

namespace QuizProject.Web.Controllers;

[Authorize]
public class QuizController : Controller
{
    private readonly IQuizService _quizService;

    public QuizController(IQuizService quizService)
    {
        _quizService = quizService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var quizzes = await _quizService.GetActiveQuizzesAsync();
        return View(quizzes);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(int quizId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        var model = await _quizService.StartAttemptAsync(quizId, userId);
        if (model is null)
        {
            TempData["Error"] = "Quiz not found or is no longer active.";
            return RedirectToAction(nameof(Index));
        }

        return View("Take", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(SubmitQuizViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please answer all questions before submitting.";
            return RedirectToAction(nameof(Index));
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        var result = await _quizService.SubmitAttemptAsync(model, userId);
        if (result is null)
        {
            TempData["Error"] = "Unable to submit quiz. It may have already been completed.";
            return RedirectToAction(nameof(Index));
        }

        return View("Results", result);
    }

    [HttpGet]
    public async Task<IActionResult> Results(int attemptId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        var result = await _quizService.GetResultAsync(attemptId, userId);
        if (result is null) return NotFound();

        return View(result);
    }
}
