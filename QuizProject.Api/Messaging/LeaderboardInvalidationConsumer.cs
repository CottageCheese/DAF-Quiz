using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Caching.Distributed;
using QuizProject.Contracts;
using System.Text.Json;

namespace QuizProject.Api.Messaging;

public sealed class LeaderboardInvalidationConsumer : BackgroundService
{
    private readonly ServiceBusProcessor _processor;
    private readonly IDistributedCache _cache;
    private readonly ILogger<LeaderboardInvalidationConsumer> _logger;

    public LeaderboardInvalidationConsumer(
        ServiceBusClient client,
        IDistributedCache cache,
        ILogger<LeaderboardInvalidationConsumer> logger)
    {
        _cache = cache;
        _logger = logger;
        _processor = client.CreateProcessor(
            topicName: "quiz-events",
            subscriptionName: "leaderboard-invalidation",
            new ServiceBusProcessorOptions { MaxConcurrentCalls = 1 });

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

        await _cache.RemoveAsync("leaderboard:top-quizzes:10");
        await _cache.RemoveAsync("leaderboard:top-users:10");

        _logger.LogInformation("Leaderboard cache invalidated after attempt {AttemptId} (quiz {QuizId})",
            evt.AttemptId, evt.QuizId);

        await args.CompleteMessageAsync(args.Message);
    }

    private Task HandleErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception,
            "Error in {Processor}: {ErrorSource}", nameof(LeaderboardInvalidationConsumer), args.ErrorSource);
        return Task.CompletedTask;
    }

}
