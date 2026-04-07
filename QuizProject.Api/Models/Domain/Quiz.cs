using System.ComponentModel.DataAnnotations;

namespace QuizProject.Api.Models.Domain;

public class Quiz
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(450)]
    public string? CreatedByUserId { get; set; }

    [MaxLength(256)]
    public string? CreatedByEmail { get; set; }

    /// <summary>
    /// When set, the quiz is published. Users can see the quiz once this date/time has passed.
    /// Null means the quiz is a draft and not visible to regular users.
    /// </summary>
    public DateTime? PublishedAt { get; set; }

    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();
}
