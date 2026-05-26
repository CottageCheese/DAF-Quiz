namespace QuizProject.Contracts;

public class QuizResultViewModel
{
    public int AttemptId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public double Percentage => ScoreHelper.Percentage(Score, TotalQuestions);
    public string Grade => ScoreHelper.Grade(Percentage);

    public List<ResultAnswerViewModel> Answers { get; set; } = new();
}
