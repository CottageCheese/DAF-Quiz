namespace QuizProject.Contracts;

public class ResultAnswerViewModel
{
    public string QuestionText { get; set; } = string.Empty;
    public string SelectedAnswerText { get; set; } = string.Empty;
    public string CorrectAnswerText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}
