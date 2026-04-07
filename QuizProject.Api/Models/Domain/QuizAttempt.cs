using Microsoft.AspNetCore.Identity;

namespace QuizProject.Api.Models.Domain;

public class QuizAttempt
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public IdentityUser User { get; set; } = null!;

    public int QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public int Score { get; set; }
    public int TotalQuestions { get; set; }

    public ICollection<QuizAttemptAnswer> AttemptAnswers { get; set; } = new List<QuizAttemptAnswer>();
}
