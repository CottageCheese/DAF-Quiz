using QuizProject.Api.Models.ViewModels;

namespace QuizProject.Api.Services;

public interface IQuizService
{
    Task<List<QuizListViewModel>> GetActiveQuizzesAsync(CancellationToken ct = default);
    Task<TakeQuizViewModel?> StartAttemptAsync(int quizId, string userId, CancellationToken ct = default);
    Task<QuizResultViewModel?> SubmitAttemptAsync(SubmitQuizViewModel submission, string userId, CancellationToken ct = default);
    Task<QuizResultViewModel?> GetResultAsync(int attemptId, string userId, CancellationToken ct = default);
}
