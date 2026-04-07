using System.ComponentModel.DataAnnotations;

namespace QuizProject.Web.Models.Domain;

public class Answer
{
    public int Id { get; set; }

    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    [Required] [MaxLength(500)] public string Text { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
}