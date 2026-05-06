using System.ComponentModel.DataAnnotations;

namespace QuizProject.Contracts;

public class UpdateQuizRequest
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public DateTime? PublishedAt { get; set; }
}
