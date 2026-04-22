namespace QuizProject.Web.Services;

public record ApiResult<T>(T? Data, bool Succeeded, string? ErrorMessage = null);