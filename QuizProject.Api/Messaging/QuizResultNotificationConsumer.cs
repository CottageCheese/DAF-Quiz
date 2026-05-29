using System.Text.Json;
using Azure.Messaging.ServiceBus;
using QuizProject.Contracts;

namespace QuizProject.Api.Messaging;

public sealed class QuizResultNotificationConsumer : BackgroundService
{
    private readonly ILogger<QuizResultNotificationConsumer> _logger;
    private readonly ServiceBusProcessor _processor;

    public QuizResultNotificationConsumer(
        ServiceBusClient client,
        ILogger<QuizResultNotificationConsumer> logger)
    {
        _logger = logger;
        _processor = client.CreateProcessor(
            "quiz-events",
            "quiz-result-notification",
            new ServiceBusProcessorOptions { MaxConcurrentCalls = 2 });

        _processor.ProcessMessageAsync += HandleMessageAsync;
        _processor.ProcessErrorAsync += HandleErrorAsync;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _processor.StartProcessingAsync(stoppingToken);
        await Task.Delay(Timeout.Infinite, stoppingToken).ContinueWith(_ => { });
        await _processor.StopProcessingAsync();
    }

    private async Task HandleMessageAsync(ProcessMessageEventArgs args)
    {
        var evt = JsonSerializer.Deserialize<QuizAttemptCompletedEvent>(args.Message.Body)!;

        // Stub: replace with IEmailSender, SendGrid, SignalR hub, etc.
        var pct = evt.TotalQuestions > 0 ? (double)evt.Score / evt.TotalQuestions * 100 : 0;
        _logger.LogInformation(
            "RESULT NOTIFICATION [stub] — '{User}' completed '{Quiz}': {Score}/{Total} ({Pct:F1}%)",
            evt.UserDisplayName, evt.QuizTitle, evt.Score, evt.TotalQuestions, pct);

        await args.CompleteMessageAsync(args.Message);
    }

    private Task HandleErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception,
            "Error in {Processor}: {ErrorSource}", nameof(QuizResultNotificationConsumer), args.ErrorSource);
        return Task.CompletedTask;
    }
}