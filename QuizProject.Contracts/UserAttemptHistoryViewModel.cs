namespace QuizProject.Contracts;

public class UserAttemptHistoryViewModel
{
    public int AttemptId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public double Percentage => ScoreHelper.Percentage(Score, TotalQuestions);
    public string Grade => ScoreHelper.Grade(Percentage);

    public DateTime CompletedAt { get; set; }
}
