namespace QuizProject.Api.Models.ViewModels;

public class SubmitQuizViewModel
{
    public int AttemptId { get; set; }
    public List<QuestionAnswerSelection> Selections { get; set; } = new();
}
