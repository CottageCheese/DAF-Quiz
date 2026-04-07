namespace QuizProject.Web.Models.ViewModels;

public class TakeQuizViewModel
{
    public int AttemptId { get; set; }
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public int TotalQuestions { get; set; }
    public List<QuizQuestionViewModel> Questions { get; set; } = new();
}

public class QuizQuestionViewModel
{
    public int QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public List<QuizAnswerViewModel> Answers { get; set; } = new();
}

public class QuizAnswerViewModel
{
    public int AnswerId { get; set; }
    public string Text { get; set; } = string.Empty;
}

public class SubmitQuizViewModel
{
    public int AttemptId { get; set; }
    public List<QuestionAnswerSelection> Selections { get; set; } = new();
}

public class QuestionAnswerSelection
{
    public int QuestionId { get; set; }
    public int SelectedAnswerId { get; set; }
}
