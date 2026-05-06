using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using QuizProject.Contracts;
using QuizProject.Tests.Integration.Infrastructure;

namespace QuizProject.Tests.Integration.Admin;

[Collection("AdminTests")]
public class AdminQuestionCrudTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private static object ValidQuestionPayload(string suffix = "") => new
    {
        text = $"What is 2+2? {suffix}",
        displayOrder = 99,
        answers = new[]
        {
            new { text = "4", isCorrect = true },
            new { text = "3", isCorrect = false },
            new { text = "5", isCorrect = false },
            new { text = "6", isCorrect = false }
        }
    };

    [Fact]
    public async Task AddQuestion_ValidRequest_Returns201()
    {
        var client = await CreateAdminClientAsync();
        var response = await client.PostAsJsonAsync(
            $"/api/admin/quizzes/{Seed.PublishedQuizId}/questions",
            ValidQuestionPayload("add"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var question = await response.Content.ReadFromJsonAsync<AdminQuestionViewModel>();
        question!.Id.Should().BeGreaterThan(0);
        question.Answers.Should().HaveCount(4);
    }

    [Fact]
    public async Task AddQuestion_NoCorrectAnswer_Returns400()
    {
        var client = await CreateAdminClientAsync();
        var response = await client.PostAsJsonAsync(
            $"/api/admin/quizzes/{Seed.PublishedQuizId}/questions",
            new
            {
                text = "No correct answer",
                displayOrder = 1,
                answers = new[]
                {
                    new { text = "Wrong A", isCorrect = false },
                    new { text = "Wrong B", isCorrect = false }
                }
            });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("correct");
    }

    [Fact]
    public async Task AddQuestion_FewerThan2Answers_Returns400()
    {
        var client = await CreateAdminClientAsync();
        var response = await client.PostAsJsonAsync(
            $"/api/admin/quizzes/{Seed.PublishedQuizId}/questions",
            new
            {
                text = "Only one answer",
                displayOrder = 1,
                answers = new[]
                {
                    new { text = "Only", isCorrect = true }
                }
            });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateQuestion_ValidRequest_Returns200()
    {
        // Add a question first, then update it
        var client = await CreateAdminClientAsync();
        var addResponse = await client.PostAsJsonAsync(
            $"/api/admin/quizzes/{Seed.PublishedQuizId}/questions",
            ValidQuestionPayload("before update"));

        var added = await addResponse.Content.ReadFromJsonAsync<AdminQuestionViewModel>();

        var updateResponse = await client.PutAsJsonAsync(
            $"/api/admin/quizzes/{Seed.PublishedQuizId}/questions/{added!.Id}",
            new
            {
                text = "Updated question text",
                displayOrder = 50,
                answers = new[]
                {
                    new { text = "Updated correct", isCorrect = true },
                    new { text = "Updated wrong", isCorrect = false }
                }
            });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResponse.Content.ReadFromJsonAsync<AdminQuestionViewModel>();
        updated!.Text.Should().Be("Updated question text");
        updated.Answers.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateQuestion_NonExistentId_Returns404()
    {
        var client = await CreateAdminClientAsync();
        var response = await client.PutAsJsonAsync(
            $"/api/admin/quizzes/{Seed.PublishedQuizId}/questions/999999",
            ValidQuestionPayload());

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteQuestion_ExistingId_Returns204()
    {
        var client = await CreateAdminClientAsync();

        // Add a question to delete
        var addResponse = await client.PostAsJsonAsync(
            $"/api/admin/quizzes/{Seed.PublishedQuizId}/questions",
            ValidQuestionPayload("to delete"));
        var added = await addResponse.Content.ReadFromJsonAsync<AdminQuestionViewModel>();

        var deleteResponse = await client.DeleteAsync(
            $"/api/admin/quizzes/{Seed.PublishedQuizId}/questions/{added!.Id}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteQuestion_NonExistentId_Returns404()
    {
        var client = await CreateAdminClientAsync();
        var response = await client.DeleteAsync(
            $"/api/admin/quizzes/{Seed.PublishedQuizId}/questions/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
