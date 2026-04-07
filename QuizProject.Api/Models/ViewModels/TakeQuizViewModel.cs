namespace QuizProject.Api.Models.ViewModels;

public class TakeQuizViewModel
{
    public int AttemptId { get; set; }
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public int TotalQuestions { get; set; }
    public List<QuizQuestionViewModel> Questions { get; set; } = new();
}
