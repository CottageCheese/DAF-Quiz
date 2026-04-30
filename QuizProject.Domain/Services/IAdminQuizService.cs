using QuizProject.Contracts;

namespace QuizProject.Domain.Services;

public interface IAdminQuizService
{
    Task<List<AdminQuizListViewModel>> GetAllQuizzesAsync(CancellationToken ct = default);
    Task<AdminQuizDetailViewModel?> GetQuizDetailAsync(int quizId, CancellationToken ct = default);
    Task<AdminQuizDetailViewModel> CreateQuizAsync(CreateQuizRequest request, string userId, string userEmail, CancellationToken ct = default);
    Task<AdminQuizDetailViewModel?> UpdateQuizAsync(int quizId, UpdateQuizRequest request, CancellationToken ct = default);
    Task<bool> DeleteQuizAsync(int quizId, CancellationToken ct = default);
    Task<AdminQuestionViewModel> AddQuestionAsync(int quizId, UpsertQuestionRequest request, CancellationToken ct = default);
    Task<AdminQuestionViewModel?> UpdateQuestionAsync(int questionId, UpsertQuestionRequest request, CancellationToken ct = default);
    Task<bool> DeleteQuestionAsync(int questionId, CancellationToken ct = default);
}
