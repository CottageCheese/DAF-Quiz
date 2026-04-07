using System.ComponentModel.DataAnnotations;

namespace QuizProject.Api.Models.ViewModels;

// ── List / summary ─────────────────────────────────────────────────────────────

public class AdminQuizListViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedByEmail { get; set; }
    public DateTime? PublishedAt { get; set; }
    public bool IsPublished { get; set; }
    public int QuestionCount { get; set; }
}

// ── Detail (with questions + correct answer highlighted) ──────────────────────

public class AdminQuizDetailViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedByEmail { get; set; }
    public DateTime? PublishedAt { get; set; }
    public bool IsPublished { get; set; }
    public List<AdminQuestionViewModel> Questions { get; set; } = [];
}

public class AdminQuestionViewModel
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public List<AdminAnswerViewModel> Answers { get; set; } = [];
}

public class AdminAnswerViewModel
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}

// ── Create / Update quiz ───────────────────────────────────────────────────────

public class CreateQuizRequest
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }
}

public class UpdateQuizRequest
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public DateTime? PublishedAt { get; set; }
}

// ── Create / Update question ──────────────────────────────────────────────────

public class UpsertQuestionRequest
{
    [Required, MaxLength(1000)]
    public string Text { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    [Required, MinLength(2)]
    public List<UpsertAnswerRequest> Answers { get; set; } = [];
}

public class UpsertAnswerRequest
{
    [Required, MaxLength(500)]
    public string Text { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
}
