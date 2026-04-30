using System.ComponentModel.DataAnnotations;

namespace QuizProject.Contracts;

public sealed record RegisterRequest
{
    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; init; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    public string Password { get; init; } = string.Empty;

    [Required, StringLength(50, MinimumLength = 2)]
    public string DisplayName { get; init; } = string.Empty;
}
