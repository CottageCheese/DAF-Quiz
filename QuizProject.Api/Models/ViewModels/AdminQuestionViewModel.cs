namespace QuizProject.Api.Models.ViewModels;

public class AdminQuestionViewModel
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public List<AdminAnswerViewModel> Answers { get; set; } = [];
}