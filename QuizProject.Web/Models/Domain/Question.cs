using System.ComponentModel.DataAnnotations;

namespace QuizProject.Web.Models.Domain;

public class Question
{
    public int Id { get; set; }

    public int QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;

    [Required, MaxLength(1000)]
    public string Text { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
}
