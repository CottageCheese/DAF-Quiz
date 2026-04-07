namespace QuizProject.Web.Models.ViewModels;

public class QuizQuestionViewModel
{
    public int QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public List<QuizAnswerViewModel> Answers { get; set; } = new();
}