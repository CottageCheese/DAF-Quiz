using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizProject.Web.Models.ViewModels;
using QuizProject.Web.Services;

namespace QuizProject.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController(IApiClient apiClient) : Controller
{
    // Quiz List

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var quizzes = await apiClient.GetAdminQuizzesAsync();
        return View(quizzes);
    }

    // Create Quiz

    [HttpGet]
    public IActionResult Create() => View(new CreateQuizFormModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateQuizFormModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var quiz = await apiClient.CreateQuizAsync(model.Title, model.Description);
        if (quiz is null)
        {
            TempData["Error"] = "Failed to create quiz. Please try again.";
            return View(model);
        }

        TempData["Success"] = $"Quiz \"{quiz.Title}\" created. Add questions below.";
        return RedirectToAction(nameof(Details), new { id = quiz.Id });
    }

    // Edit Quiz

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var quiz = await apiClient.GetAdminQuizAsync(id);
        if (quiz is null) return NotFound();

        return View(new EditQuizFormModel
        {
            Id = quiz.Id,
            Title = quiz.Title,
            Description = quiz.Description,
            PublishedAt = quiz.PublishedAt
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditQuizFormModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var quiz = await apiClient.UpdateQuizAsync(model.Id, model.Title, model.Description, model.PublishedAt);
        if (quiz is null)
        {
            TempData["Error"] = "Failed to update quiz. Please try again.";
            return View(model);
        }

        TempData["Success"] = "Quiz updated successfully.";
        return RedirectToAction(nameof(Details), new { id = quiz.Id });
    }

    // Quiz Details

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var quiz = await apiClient.GetAdminQuizAsync(id);
        if (quiz is null) return NotFound();
        return View(quiz);
    }

    // Delete Quiz

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQuiz(int id)
    {
        var deleted = await apiClient.DeleteQuizAsync(id);
        if (!deleted)
        {
            TempData["Error"] = "Failed to delete quiz.";
        }
        else
        {
            TempData["Success"] = "Quiz deleted successfully.";
        }

        return RedirectToAction(nameof(Index));
    }

    // Add Question

    [HttpGet]
    public async Task<IActionResult> AddQuestion(int quizId)
    {
        var quiz = await apiClient.GetAdminQuizAsync(quizId);
        if (quiz is null) return NotFound();

        return View(new QuestionFormModel
        {
            QuizId = quizId,
            DisplayOrder = quiz.Questions.Count + 1
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddQuestion(QuestionFormModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var answers = BuildAnswerList(model);
        if (answers is null)
        {
            ModelState.AddModelError(nameof(model.CorrectAnswerIndex),
                "Please select a valid correct answer.");
            return View(model);
        }

        var question = await apiClient.AddQuestionAsync(model.QuizId, model.Text, model.DisplayOrder, answers);
        if (question is null)
        {
            TempData["Error"] = "Failed to add question. Please try again.";
            return View(model);
        }

        TempData["Success"] = "Question added.";
        return RedirectToAction(nameof(Details), new { id = model.QuizId });
    }

    // Edit Question

    [HttpGet]
    public async Task<IActionResult> EditQuestion(int quizId, int questionId)
    {
        var quiz = await apiClient.GetAdminQuizAsync(quizId);
        var question = quiz?.Questions.FirstOrDefault(q => q.Id == questionId);
        if (question is null) return NotFound();

        var answers = question.Answers.ToList();
        var correctIndex = answers.FindIndex(a => a.IsCorrect);

        return View(new QuestionFormModel
        {
            QuizId = quizId,
            QuestionId = questionId,
            Text = question.Text,
            DisplayOrder = question.DisplayOrder,
            Answer1 = answers.Count > 0 ? answers[0].Text : string.Empty,
            Answer2 = answers.Count > 1 ? answers[1].Text : string.Empty,
            Answer3 = answers.Count > 2 ? answers[2].Text : null,
            Answer4 = answers.Count > 3 ? answers[3].Text : null,
            CorrectAnswerIndex = correctIndex >= 0 ? correctIndex : 0
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditQuestion(QuestionFormModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var answers = BuildAnswerList(model);
        if (answers is null)
        {
            ModelState.AddModelError(nameof(model.CorrectAnswerIndex),
                "Please select a valid correct answer.");
            return View(model);
        }

        var question = await apiClient.UpdateQuestionAsync(
            model.QuizId, model.QuestionId, model.Text, model.DisplayOrder, answers);

        if (question is null)
        {
            TempData["Error"] = "Failed to update question. Please try again.";
            return View(model);
        }

        TempData["Success"] = "Question updated.";
        return RedirectToAction(nameof(Details), new { id = model.QuizId });
    }

    // Delete Question

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQuestion(int quizId, int questionId)
    {
        var deleted = await apiClient.DeleteQuestionAsync(quizId, questionId);
        if (!deleted) TempData["Error"] = "Failed to delete question.";
        else TempData["Success"] = "Question deleted.";

        return RedirectToAction(nameof(Details), new { id = quizId });
    }

    // Helpers

    private static List<(string Text, bool IsCorrect)>? BuildAnswerList(QuestionFormModel model)
    {
        var texts = new List<string?> { model.Answer1, model.Answer2, model.Answer3, model.Answer4 };
        var nonEmpty = texts.Where(t => !string.IsNullOrWhiteSpace(t)).ToList();

        if (nonEmpty.Count < 2) return null;
        if (model.CorrectAnswerIndex < 0 || model.CorrectAnswerIndex >= nonEmpty.Count) return null;

        return nonEmpty.Select((t, i) => (t!, i == model.CorrectAnswerIndex)).ToList();
    }
}
