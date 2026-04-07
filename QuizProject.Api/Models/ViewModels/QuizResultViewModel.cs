namespace QuizProject.Api.Models.ViewModels;

public class QuizResultViewModel
{
    public int AttemptId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public double Percentage => TotalQuestions > 0 ? Math.Round((double)Score / TotalQuestions * 100, 1) : 0;

    public string Grade => Percentage switch
    {
        >= 90 => "Excellent",
        >= 70 => "Good",
        >= 50 => "Pass",
        _ => "Needs Improvement"
    };

    public List<ResultAnswerViewModel> Answers { get; set; } = new();
}
