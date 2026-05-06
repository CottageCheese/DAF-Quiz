namespace QuizProject.Contracts;

public class AdminAnswerViewModel
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}
