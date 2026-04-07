using QuizProject.Api.Models.ViewModels;

namespace QuizProject.Api.Services;

public interface IQuizService
{
    Task<List<QuizListViewModel>> GetActiveQuizzesAsync();
    Task<TakeQuizViewModel?> StartAttemptAsync(int quizId, string userId);
    Task<QuizResultViewModel?> SubmitAttemptAsync(SubmitQuizViewModel submission, string userId);
    Task<QuizResultViewModel?> GetResultAsync(int attemptId, string userId);
}
