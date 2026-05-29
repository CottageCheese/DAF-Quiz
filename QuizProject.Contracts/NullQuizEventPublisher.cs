namespace QuizProject.Contracts;

public sealed class NullQuizEventPublisher : IQuizEventPublisher
{
    public Task PublishQuizAttemptCompletedAsync(QuizAttemptCompletedEvent evt, CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }
}