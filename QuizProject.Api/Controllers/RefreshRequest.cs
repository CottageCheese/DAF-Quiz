namespace QuizProject.Api.Controllers;

public sealed record RefreshRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}