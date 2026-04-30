using System.ComponentModel.DataAnnotations;

namespace QuizProject.Contracts;

public class UpsertAnswerRequest
{
    [Required, MaxLength(500)]
    public string Text { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
}
