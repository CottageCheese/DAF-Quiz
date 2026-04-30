namespace QuizProject.Contracts;

public sealed record RefreshRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}
