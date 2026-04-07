using System.ComponentModel.DataAnnotations;

namespace QuizProject.Web.Models.ViewModels;

// ── Quiz list / detail (mirrors API response) ──────────────────────────────────

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

// ── Form models ────────────────────────────────────────────────────────────────

public class CreateQuizFormModel
{
    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }
}

public class EditQuizFormModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Display(Name = "Publish Date (UTC)")]
    public DateTime? PublishedAt { get; set; }
}

public class QuestionFormModel
{
    public int QuizId { get; set; }

    public int QuestionId { get; set; }

    [Required(ErrorMessage = "Question text is required.")]
    [MaxLength(1000)]
    [Display(Name = "Question")]
    public string Text { get; set; } = string.Empty;

    [Display(Name = "Display Order")]
    public int DisplayOrder { get; set; } = 1;

    [Required]
    [MaxLength(500)]
    [Display(Name = "Answer 1")]
    public string Answer1 { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    [Display(Name = "Answer 2")]
    public string Answer2 { get; set; } = string.Empty;

    [MaxLength(500)]
    [Display(Name = "Answer 3")]
    public string? Answer3 { get; set; }

    [MaxLength(500)]
    [Display(Name = "Answer 4")]
    public string? Answer4 { get; set; }

    [Required(ErrorMessage = "Please select the correct answer.")]
    [Display(Name = "Correct Answer")]
    public int CorrectAnswerIndex { get; set; } = 0;
}
