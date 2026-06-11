namespace QuizProject.Web.Common.Extensions;

/// <summary>Options for configuring shared web infrastructure per site.</summary>
public sealed record WebCommonOptions
{
    public string SessionCookieName { get; init; } = ".QuizProject.Session";
    public string AuthCookieName { get; init; } = ".QuizProject.Auth";
}
