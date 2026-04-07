namespace QuizProject.Web.Models.ViewModels;

public class TopUserViewModel
{
    public int Rank { get; set; }
    public string UserName { get; set; } = string.Empty;
    public double BestScorePercent { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
}