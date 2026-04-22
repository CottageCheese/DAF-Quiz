namespace QuizProject.Web.Models.ViewModels;

// Quiz list / detail (mirrors API response)

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

// Form models