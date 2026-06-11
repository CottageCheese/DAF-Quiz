using System.ComponentModel.DataAnnotations;

namespace QuizProject.Web.Admin.Models.ViewModels;

public class CreateQuizFormModel
{
    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)] public string? Description { get; set; }
}
