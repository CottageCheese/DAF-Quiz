using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using QuizProject.Api.Models.ViewModels;
using QuizProject.Api.Services;
using QuizProject.Tests.Integration.Infrastructure;

namespace QuizProject.Tests.Integration.Admin;

[Collection("AdminTests")]
public class AdminQuizCrudTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private void EvictQuizCache()
    {
        var cache = Factory.Services.GetRequiredService<IMemoryCache>();
        cache.Remove(QuizService.ActiveQuizzesCacheKey);
    }

    [Fact]
    public async Task GetAllQuizzes_Admin_Returns200WithDraftsAndPublished()
    {
        var client = await CreateAdminClientAsync();
        var response = await client.GetAsync("/api/admin/quizzes");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var quizzes = await response.Content.ReadFromJsonAsync<List<AdminQuizListViewModel>>();
        quizzes.Should().Contain(q => q.Id == Seed.PublishedQuizId);
        quizzes.Should().Contain(q => q.Id == Seed.DraftQuizId);
    }

    [Fact]
    public async Task GetAllQuizzes_RegularUser_Returns403()
    {
        var client = await CreateUserClientAsync();
        var response = await client.GetAsync("/api/admin/quizzes");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllQuizzes_NoAuth_Returns401()
    {
        var response = await Client.GetAsync("/api/admin/quizzes");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetQuiz_ExistingId_Returns200WithQuestionsAndAnswers()
    {
        var client = await CreateAdminClientAsync();
        var response = await client.GetAsync($"/api/admin/quizzes/{Seed.PublishedQuizId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var quiz = await response.Content.ReadFromJsonAsync<AdminQuizDetailViewModel>();
        quiz!.Id.Should().Be(Seed.PublishedQuizId);
        quiz.Questions.Should().HaveCount(3);
        quiz.Questions.All(q => q.Answers.Count > 0).Should().BeTrue();
        // Admin view exposes IsCorrect
        quiz.Questions.SelectMany(q => q.Answers).Any(a => a.IsCorrect).Should().BeTrue();
    }

    [Fact]
    public async Task GetQuiz_NonExistentId_Returns404()
    {
        var client = await CreateAdminClientAsync();
        var response = await client.GetAsync("/api/admin/quizzes/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateQuiz_ValidRequest_Returns201WithId()
    {
        var client = await CreateAdminClientAsync();
        var response = await client.PostAsJsonAsync("/api/admin/quizzes", new
        {
            title = "New Admin Quiz",
            description = "Created in test"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var quiz = await response.Content.ReadFromJsonAsync<AdminQuizDetailViewModel>();
        quiz!.Id.Should().BeGreaterThan(0);
        quiz.IsPublished.Should().BeFalse();
    }

    [Fact]
    public async Task CreateQuiz_MissingTitle_Returns400()
    {
        var client = await CreateAdminClientAsync();
        var response = await client.PostAsJsonAsync("/api/admin/quizzes", new
        {
            title = "",
            description = "No title"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateQuiz_TitleTooLong_Returns400()
    {
        var client = await CreateAdminClientAsync();
        var response = await client.PostAsJsonAsync("/api/admin/quizzes", new
        {
            title = new string('A', 201),
            description = "Title too long"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateQuiz_SetPublishedAt_IsPublishedTrue()
    {
        // Create a fresh quiz first
        var client = await CreateAdminClientAsync();
        var createResponse = await client.PostAsJsonAsync("/api/admin/quizzes", new
        {
            title = "To Publish",
            description = "Will be published"
        });
        var created = await createResponse.Content.ReadFromJsonAsync<AdminQuizDetailViewModel>();

        EvictQuizCache();

        var updateResponse = await client.PutAsJsonAsync($"/api/admin/quizzes/{created!.Id}", new
        {
            title = "To Publish",
            publishedAt = DateTime.UtcNow.AddHours(-1).ToString("o")
        });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResponse.Content.ReadFromJsonAsync<AdminQuizDetailViewModel>();
        updated!.IsPublished.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateQuiz_ClearPublishedAt_IsPublishedFalse()
    {
        var client = await CreateAdminClientAsync();
        // Start with the already-published quiz — update to set publishedAt null
        var updateResponse = await client.PutAsJsonAsync($"/api/admin/quizzes/{Seed.PublishedQuizId}", new
        {
            title = Seed.PublishedQuizTitle,
            publishedAt = (string?)null
        });

        EvictQuizCache();

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResponse.Content.ReadFromJsonAsync<AdminQuizDetailViewModel>();
        updated!.IsPublished.Should().BeFalse();

        // Restore for other tests
        await client.PutAsJsonAsync($"/api/admin/quizzes/{Seed.PublishedQuizId}", new
        {
            title = Seed.PublishedQuizTitle,
            publishedAt = DateTime.UtcNow.AddHours(-1).ToString("o")
        });
        EvictQuizCache();
    }

    [Fact]
    public async Task UpdateQuiz_NonExistentId_Returns404()
    {
        var client = await CreateAdminClientAsync();
        var response = await client.PutAsJsonAsync("/api/admin/quizzes/999999", new
        {
            title = "Ghost Quiz"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteQuiz_ExistingId_Returns204()
    {
        var client = await CreateAdminClientAsync();

        // Create a quiz to delete
        var createResponse = await client.PostAsJsonAsync("/api/admin/quizzes", new
        {
            title = "To Delete",
            description = "Will be deleted"
        });
        var created = await createResponse.Content.ReadFromJsonAsync<AdminQuizDetailViewModel>();

        var deleteResponse = await client.DeleteAsync($"/api/admin/quizzes/{created!.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Subsequent GET returns 404
        var getResponse = await client.GetAsync($"/api/admin/quizzes/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteQuiz_NonExistentId_Returns404()
    {
        var client = await CreateAdminClientAsync();
        var response = await client.DeleteAsync("/api/admin/quizzes/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
