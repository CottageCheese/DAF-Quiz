using QuizProject.Contracts;

namespace QuizProject.Web.Models.ViewModels;

public class DashboardViewModel
{
    public string UserName { get; set; } = string.Empty;

    public int TotalAttempts { get; set; }
    public int PassedCount { get; set; }
    public double PassRate { get; set; }
    public double BestPercent { get; set; }
    public string? BestQuizTitle { get; set; }

    public List<UserAttemptHistoryViewModel> RecentAttempts { get; set; } = new();
    public List<TopUserViewModel> TopScorers { get; set; } = new();
}
