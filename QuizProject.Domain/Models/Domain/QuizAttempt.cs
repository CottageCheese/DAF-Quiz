namespace QuizProject.Domain.Models.Domain;

public class QuizAttempt
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public int QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public int Score { get; set; }
    public int TotalQuestions { get; set; }

    /// <summary>In-memory only — do not use in EF LINQ queries.</summary>
    public bool IsCompleted => CompletedAt != null;

    /// <summary>In-memory only — do not use in EF LINQ queries.</summary>
    public double ScorePercentage => TotalQuestions > 0 ? (double)Score / TotalQuestions * 100 : 0;

    public ICollection<QuizAttemptAnswer> AttemptAnswers { get; set; } = new List<QuizAttemptAnswer>();
}