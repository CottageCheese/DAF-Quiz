using QuizProject.Contracts;

namespace QuizProject.Web.Models.ViewModels;

public class LeaderboardViewModel
{
    public List<TopQuizViewModel> TopQuizzes { get; set; } = new();
    public List<TopUserViewModel> TopUsers { get; set; } = new();
}