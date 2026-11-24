namespace QuizProject.Web.Models.ViewModels;

public class EmptyStateModel
{
    public string Title { get; set; } = "Nothing here yet";
    public string? Message { get; set; }
    public string? CtaText { get; set; }
    public string? CtaHref { get; set; }
    public string? IconSvg { get; set; }
}

public class ErrorStateModel
{
    public string? Title { get; set; }
    public string? Message { get; set; }
    public string? RetryText { get; set; }
    public string? RetryHref { get; set; }
}
