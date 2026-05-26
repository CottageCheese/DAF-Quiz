namespace QuizProject.Contracts;

public static class ScoreHelper
{
    public static double Percentage(int score, int total) =>
        total > 0 ? Math.Round((double)score / total * 100, 1) : 0;

    public static string Grade(double percentage) => percentage switch
    {
        >= 90 => "Excellent",
        >= 70 => "Good",
        >= 50 => "Pass",
        _     => "Needs Improvement"
    };
}
