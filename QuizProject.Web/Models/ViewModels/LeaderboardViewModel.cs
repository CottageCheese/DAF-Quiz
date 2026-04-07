namespace QuizProject.Web.Models.ViewModels;

public class LeaderboardViewModel
{
    public List<TopQuizViewModel> TopQuizzes { get; set; } = new();
    public List<TopUserViewModel> TopUsers { get; set; } = new();
}

public class TopQuizViewModel
{
    public int Rank { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public int AttemptCount { get; set; }
}

public class TopUserViewModel
{
    public int Rank { get; set; }
    public string UserName { get; set; } = string.Empty;
    public double BestScorePercent { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
}
