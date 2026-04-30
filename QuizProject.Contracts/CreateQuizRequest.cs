using System.ComponentModel.DataAnnotations;

namespace QuizProject.Contracts;

public class CreateQuizRequest
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }
}
