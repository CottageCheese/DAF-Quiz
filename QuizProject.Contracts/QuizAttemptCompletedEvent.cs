namespace QuizProject.Contracts;

public record QuizAttemptCompletedEvent
{
    public int AttemptId { get; init; }
    public int QuizId { get; init; }
    public string QuizTitle { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public string UserDisplayName { get; init; } = string.Empty;
    public int Score { get; init; }
    public int TotalQuestions { get; init; }
    public DateTime CompletedAt { get; init; }
}
