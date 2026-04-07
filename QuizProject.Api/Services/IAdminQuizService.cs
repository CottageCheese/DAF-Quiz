using QuizProject.Api.Models.ViewModels;

namespace QuizProject.Api.Services;

public interface IAdminQuizService
{
    Task<List<AdminQuizListViewModel>> GetAllQuizzesAsync();
    Task<AdminQuizDetailViewModel?> GetQuizDetailAsync(int quizId);
    Task<AdminQuizDetailViewModel> CreateQuizAsync(CreateQuizRequest request, string userId, string userEmail);
    Task<AdminQuizDetailViewModel?> UpdateQuizAsync(int quizId, UpdateQuizRequest request);
    Task<bool> DeleteQuizAsync(int quizId);
    Task<AdminQuestionViewModel> AddQuestionAsync(int quizId, UpsertQuestionRequest request);
    Task<AdminQuestionViewModel?> UpdateQuestionAsync(int questionId, UpsertQuestionRequest request);
    Task<bool> DeleteQuestionAsync(int questionId);
}
