using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using QuizProject.Domain.Exceptions;

namespace QuizProject.Api.Infrastructure;

public class DomainValidationExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not DomainValidationException ex) return;
        context.Result = new BadRequestObjectResult(new { message = ex.Message });
        context.ExceptionHandled = true;
    }
}
