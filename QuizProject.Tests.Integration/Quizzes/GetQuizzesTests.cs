using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using QuizProject.Api.Models.ViewModels;
using QuizProject.Tests.Integration.Infrastructure;

namespace QuizProject.Tests.Integration.Quizzes;

[Collection("QuizTests")]
public class GetQuizzesTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetQuizzes_Authenticated_Returns200WithList()
    {
        var client = await CreateUserClientAsync();
        var response = await client.GetAsync("/api/quizzes");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var quizzes = await response.Content.ReadFromJsonAsync<List<QuizListViewModel>>();
        quizzes.Should().NotBeNull();
        quizzes.Should().Contain(q => q.Id == Seed.PublishedQuizId);
    }

    [Fact]
    public async Task GetQuizzes_DoesNotReturnDraftQuiz()
    {
        var client = await CreateUserClientAsync();
        var response = await client.GetAsync("/api/quizzes");

        var quizzes = await response.Content.ReadFromJsonAsync<List<QuizListViewModel>>();
        quizzes.Should().NotContain(q => q.Id == Seed.DraftQuizId);
    }

    [Fact]
    public async Task GetQuizzes_NoAuth_Returns401()
    {
        var response = await Client.GetAsync("/api/quizzes");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetQuizzes_ResponseShape_HasAllRequiredFields()
    {
        var client = await CreateUserClientAsync();
        var response = await client.GetAsync("/api/quizzes");

        var quizzes = await response.Content.ReadFromJsonAsync<List<QuizListViewModel>>();
        var quiz = quizzes!.First(q => q.Id == Seed.PublishedQuizId);

        quiz.Title.Should().Be(Seed.PublishedQuizTitle);
        quiz.QuestionCount.Should().Be(3);
        quiz.CreatedAt.Should().NotBe(default);
    }
}
