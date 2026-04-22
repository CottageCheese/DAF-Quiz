using System.ComponentModel.DataAnnotations;

namespace QuizProject.Web.Models.ViewModels;

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