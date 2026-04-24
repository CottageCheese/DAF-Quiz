using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using QuizProject.Api.Models.ViewModels;
using QuizProject.Tests.Integration.Infrastructure;

namespace QuizProject.Tests.Integration.Quizzes;

[Collection("QuizTests")]
public class StartAttemptTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task StartAttempt_PublishedQuiz_Returns200WithViewModel()
    {
        var client = await CreateUserClientAsync();
        var response = await client.PostAsync($"/api/quizzes/{Seed.PublishedQuizId}/start", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var model = await response.Content.ReadFromJsonAsync<TakeQuizViewModel>();
        model!.AttemptId.Should().BeGreaterThan(0);
        model.QuizId.Should().Be(Seed.PublishedQuizId);
        model.QuizTitle.Should().Be(Seed.PublishedQuizTitle);
        model.TotalQuestions.Should().Be(3);
        model.Questions.Should().HaveCount(3);
    }

    [Fact]
    public async Task StartAttempt_EachQuestionHasAnswers_IsCorrectNotExposed()
    {
        var client = await CreateUserClientAsync();
        var response = await client.PostAsync($"/api/quizzes/{Seed.PublishedQuizId}/start", null);

        var raw = await response.Content.ReadAsStringAsync();
        var model = System.Text.Json.JsonSerializer.Deserialize<TakeQuizViewModel>(raw,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        foreach (var question in model!.Questions)
        {
            question.Answers.Should().HaveCountGreaterThanOrEqualTo(2);
        }

        // Confirm IsCorrect is NOT present in the raw JSON response
        raw.Should().NotContain("\"isCorrect\"", "isCorrect must not be exposed to users");
    }

    [Fact]
    public async Task StartAttempt_DraftQuiz_Returns404()
    {
        var client = await CreateUserClientAsync();
        var response = await client.PostAsync($"/api/quizzes/{Seed.DraftQuizId}/start", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task StartAttempt_NonExistentQuiz_Returns404()
    {
        var client = await CreateUserClientAsync();
        var response = await client.PostAsync("/api/quizzes/999999/start", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task StartAttempt_NoAuth_Returns401()
    {
        var response = await Client.PostAsync($"/api/quizzes/{Seed.PublishedQuizId}/start", null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
