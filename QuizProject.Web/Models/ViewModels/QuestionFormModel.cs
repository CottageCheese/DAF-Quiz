using System.ComponentModel.DataAnnotations;

namespace QuizProject.Web.Models.ViewModels;

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