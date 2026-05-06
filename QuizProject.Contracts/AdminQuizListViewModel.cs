namespace QuizProject.Contracts;

// List / summary

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

// Detail (with questions + correct answer highlighted)

// Create / Update quiz

// Create / Update question
