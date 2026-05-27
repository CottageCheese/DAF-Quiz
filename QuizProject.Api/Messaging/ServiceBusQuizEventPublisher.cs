using Azure.Messaging.ServiceBus;
using QuizProject.Contracts;
using System.Text.Json;

namespace QuizProject.Api.Messaging;

public sealed class ServiceBusQuizEventPublisher : IQuizEventPublisher, IAsyncDisposable
{
    private readonly ServiceBusSender _sender;
    private readonly ILogger<ServiceBusQuizEventPublisher> _logger;

    public ServiceBusQuizEventPublisher(ServiceBusClient client, ILogger<ServiceBusQuizEventPublisher> logger)
    {
        _sender = client.CreateSender("quiz-events");
        _logger = logger;
    }

    public async Task PublishQuizAttemptCompletedAsync(QuizAttemptCompletedEvent evt, CancellationToken ct = default)
    {
        var body = JsonSerializer.SerializeToUtf8Bytes(evt);
        var message = new ServiceBusMessage(body)
        {
            ContentType = "application/json",
            Subject = nameof(QuizAttemptCompletedEvent),
            MessageId = $"attempt-{evt.AttemptId}"
        };
        await _sender.SendMessageAsync(message, ct);
        _logger.LogInformation("Published {Event} for attempt {AttemptId}", nameof(QuizAttemptCompletedEvent), evt.AttemptId);
    }

    public async ValueTask DisposeAsync() => await _sender.DisposeAsync();
}
