namespace QuizProject.Contracts;

public interface IQuizEventPublisher
{
    Task PublishQuizAttemptCompletedAsync(QuizAttemptCompletedEvent evt, CancellationToken ct = default);
}