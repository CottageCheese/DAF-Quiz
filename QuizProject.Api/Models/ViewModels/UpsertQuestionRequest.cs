using System.ComponentModel.DataAnnotations;

namespace QuizProject.Api.Models.ViewModels;

public class UpsertQuestionRequest
{
    [Required, MaxLength(1000)]
    public string Text { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    [Required, MinLength(2)]
    public List<UpsertAnswerRequest> Answers { get; set; } = [];
}