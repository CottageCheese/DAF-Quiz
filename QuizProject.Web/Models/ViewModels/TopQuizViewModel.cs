namespace QuizProject.Web.Models.ViewModels;

public class TopQuizViewModel
{
    public int Rank { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public int AttemptCount { get; set; }
}