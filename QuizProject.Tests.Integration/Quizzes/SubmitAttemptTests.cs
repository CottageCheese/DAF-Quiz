using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using QuizProject.Api.Models.ViewModels;
using QuizProject.Tests.Integration.Infrastructure;

namespace QuizProject.Tests.Integration.Quizzes;

[Collection("QuizTests")]
public class SubmitAttemptTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private async Task<(HttpClient client, int attemptId, List<QuizAnswerViewModel>[] answers)> StartFreshAttemptAsync()
    {
        var client = await CreateUserClientAsync();
        var startResponse = await client.PostAsync($"/api/quizzes/{Seed.PublishedQuizId}/start", null);
        startResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var model = await startResponse.Content.ReadFromJsonAsync<TakeQuizViewModel>();
        var answers = model!.Questions.Select(q => q.Answers).ToArray();
        return (client, model.AttemptId, answers);
    }

    [Fact]
    public async Task SubmitAttempt_AllCorrect_ReturnsScore3of3()
    {
        var (client, attemptId, _) = await StartFreshAttemptAsync();

        var selections = Seed.QuestionIds
            .Zip(Seed.CorrectAnswerIds, (qId, aId) => new { questionId = qId, selectedAnswerId = aId })
            .ToList();

        var response = await client.PostAsJsonAsync($"/api/quizzes/attempts/{attemptId}/submit", selections);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<QuizResultViewModel>();
        result!.Score.Should().Be(3);
        result.TotalQuestions.Should().Be(3);
        result.Percentage.Should().Be(100);
        result.Grade.Should().Be("Excellent");
    }

    [Fact]
    public async Task SubmitAttempt_AllWrong_ReturnsScore0()
    {
        var (client, attemptId, _) = await StartFreshAttemptAsync();

        var selections = Seed.QuestionIds
            .Zip(Seed.WrongAnswerIds, (qId, aId) => new { questionId = qId, selectedAnswerId = aId })
            .ToList();

        var response = await client.PostAsJsonAsync($"/api/quizzes/attempts/{attemptId}/submit", selections);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<QuizResultViewModel>();
        result!.Score.Should().Be(0);
        result.Grade.Should().Be("Needs Improvement");
    }

    [Fact]
    public async Task SubmitAttempt_ReturnsAnswerDetail()
    {
        var (client, attemptId, _) = await StartFreshAttemptAsync();

        var selections = Seed.QuestionIds
            .Zip(Seed.CorrectAnswerIds, (qId, aId) => new { questionId = qId, selectedAnswerId = aId })
            .ToList();

        var response = await client.PostAsJsonAsync($"/api/quizzes/attempts/{attemptId}/submit", selections);

        var result = await response.Content.ReadFromJsonAsync<QuizResultViewModel>();
        result!.Answers.Should().HaveCount(3);
        foreach (var answer in result.Answers)
        {
            answer.QuestionText.Should().NotBeNullOrWhiteSpace();
            answer.SelectedAnswerText.Should().NotBeNullOrWhiteSpace();
            answer.CorrectAnswerText.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public async Task SubmitAttempt_AlreadyCompleted_Returns404()
    {
        var (client, attemptId, _) = await StartFreshAttemptAsync();

        var selections = Seed.QuestionIds
            .Zip(Seed.CorrectAnswerIds, (qId, aId) => new { questionId = qId, selectedAnswerId = aId })
            .ToList();

        // First submit
        await client.PostAsJsonAsync($"/api/quizzes/attempts/{attemptId}/submit", selections);

        // Second submit — attempt already completed
        var response = await client.PostAsJsonAsync($"/api/quizzes/attempts/{attemptId}/submit", selections);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SubmitAttempt_WrongUser_Returns404()
    {
        // Start attempt as primary user
        var (_, attemptId, _) = await StartFreshAttemptAsync();

        // Submit as a different user
        var otherClient = await CreateAdminClientAsync();
        var selections = Seed.QuestionIds
            .Zip(Seed.CorrectAnswerIds, (qId, aId) => new { questionId = qId, selectedAnswerId = aId })
            .ToList();

        var response = await otherClient.PostAsJsonAsync($"/api/quizzes/attempts/{attemptId}/submit", selections);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SubmitAttempt_InvalidAttemptId_Returns404()
    {
        var client = await CreateUserClientAsync();
        var selections = new[] { new { questionId = 1, selectedAnswerId = 1 } };

        var response = await client.PostAsJsonAsync("/api/quizzes/attempts/999999/submit", selections);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SubmitAttempt_NoAuth_Returns401()
    {
        var selections = new[] { new { questionId = 1, selectedAnswerId = 1 } };
        var response = await Client.PostAsJsonAsync("/api/quizzes/attempts/1/submit", selections);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
