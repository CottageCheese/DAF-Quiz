using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using QuizProject.Contracts;
using QuizProject.Tests.Integration.Infrastructure;

namespace QuizProject.Tests.Integration.Quizzes;

[Collection("QuizTests")]
public class GetResultTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetResult_CompletedAttempt_Returns200WithCorrectData()
    {
        var client = await CreateUserClientAsync();
        var response = await client.GetAsync($"/api/quizzes/attempts/{Seed.CompletedAttemptId}/result");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<QuizResultViewModel>();
        result!.AttemptId.Should().Be(Seed.CompletedAttemptId);
        result.QuizTitle.Should().Be(Seed.PublishedQuizTitle);
        result.Score.Should().Be(3);
        result.TotalQuestions.Should().Be(3);
        result.Percentage.Should().Be(100);
        result.Grade.Should().Be("Excellent");
    }

    [Fact]
    public async Task GetResult_WrongUser_Returns404()
    {
        // The completed attempt belongs to the regular user; try as admin
        var adminClient = await CreateAdminClientAsync();
        var response = await adminClient.GetAsync($"/api/quizzes/attempts/{Seed.CompletedAttemptId}/result");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetResult_NonExistentAttempt_Returns404()
    {
        var client = await CreateUserClientAsync();
        var response = await client.GetAsync("/api/quizzes/attempts/999999/result");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetResult_NoAuth_Returns401()
    {
        var response = await Client.GetAsync($"/api/quizzes/attempts/{Seed.CompletedAttemptId}/result");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
