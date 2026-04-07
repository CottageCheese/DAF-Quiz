namespace QuizProject.Web.Models.Domain;

public class QuizAttemptAnswer
{
    public int Id { get; set; }

    public int AttemptId { get; set; }
    public QuizAttempt Attempt { get; set; } = null!;

    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    public int SelectedAnswerId { get; set; }
    public Answer SelectedAnswer { get; set; } = null!;

    public bool IsCorrect { get; set; }
}
