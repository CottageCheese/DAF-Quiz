namespace QuizProject.Web.Common.Services;

public record ApiResult<T>(T? Data, bool Succeeded, string? ErrorMessage = null);
