using QuizProject.Contracts;
using QuizProject.Domain.Services;
using QuizProject.Domain.Tests.Infrastructure;

namespace QuizProject.Domain.Tests.Services;

public class AdminQuizServiceTests : DomainTestBase
{
    private readonly AdminQuizService _svc;

    public AdminQuizServiceTests()
    {
        _svc = new AdminQuizService(QuizRepo, QuestionRepo, AnswerRepo, AttemptRepo, AttemptAnswerRepo, Cache);
    }

    [Fact]
    public async Task GetAllQuizzesAsync_ReturnsBothPublishedAndDraft()
    {
        var seed = await DomainTestSeeder.SeedAsync(Db);

        var result = await _svc.GetAllQuizzesAsync();

        result.Should().HaveCount(2);
        result.Should().Contain(q => q.Title == seed.PublishedQuiz.Title);
        result.Should().Contain(q => q.Title == seed.DraftQuiz.Title);
    }

    [Fact]
    public async Task GetAllQuizzesAsync_OrderedByCreatedAtDescending()
    {
        await DomainTestSeeder.SeedAsync(Db);

        var result = await _svc.GetAllQuizzesAsync();

        result.Should().BeInDescendingOrder(q => q.CreatedAt);
    }

    [Fact]
    public async Task CreateQuizAsync_PersistsWithCorrectMetadata()
    {
        var request = new CreateQuizRequest { Title = "New Quiz", Description = "Desc" };

        var result = await _svc.CreateQuizAsync(request, "user-1", "creator@test.local");

        result.Title.Should().Be("New Quiz");
        result.Description.Should().Be("Desc");
        (await _svc.GetAllQuizzesAsync()).Should().ContainSingle(q => q.Title == "New Quiz");
    }

    [Fact]
    public async Task UpdateQuizAsync_UpdatesFields()
    {
        var seed = await DomainTestSeeder.SeedAsync(Db);
        var request = new UpdateQuizRequest
        {
            Title = "Updated Title",
            Description = "Updated Desc",
            PublishedAt = DateTime.UtcNow.AddDays(-1)
        };

        var result = await _svc.UpdateQuizAsync(seed.PublishedQuiz.Id, request);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Updated Title");
        result.Description.Should().Be("Updated Desc");
        result.IsPublished.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateQuizAsync_NonExistent_ReturnsNull()
    {
        var result = await _svc.UpdateQuizAsync(9999, new UpdateQuizRequest { Title = "x" });

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteQuizAsync_RemovesQuiz()
    {
        var seed = await DomainTestSeeder.SeedAsync(Db);

        var deleted = await _svc.DeleteQuizAsync(seed.DraftQuiz.Id);

        deleted.Should().BeTrue();
        (await _svc.GetAllQuizzesAsync()).Should().NotContain(q => q.Title == seed.DraftQuiz.Title);
    }

    [Fact]
    public async Task DeleteQuizAsync_NonExistent_ReturnsFalse()
    {
        var result = await _svc.DeleteQuizAsync(9999);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddQuestionAsync_PersistsQuestionAndAnswers()
    {
        var seed = await DomainTestSeeder.SeedAsync(Db);
        var request = new UpsertQuestionRequest
        {
            Text = "New Question",
            DisplayOrder = 3,
            Answers =
            [
                new UpsertAnswerRequest { Text = "Answer A", IsCorrect = true },
                new UpsertAnswerRequest { Text = "Answer B", IsCorrect = false }
            ]
        };

        var result = await _svc.AddQuestionAsync(seed.PublishedQuiz.Id, request);

        result.Text.Should().Be("New Question");
        result.Answers.Should().HaveCount(2);
        result.Answers.Should().ContainSingle(a => a.IsCorrect);
    }

    [Fact]
    public async Task UpdateQuestionAsync_ReplacesAllAnswers()
    {
        var seed = await DomainTestSeeder.SeedAsync(Db);
        var original = await _svc.GetQuizDetailAsync(seed.PublishedQuiz.Id);
        var questionId = original!.Questions.First().Id;

        var request = new UpsertQuestionRequest
        {
            Text = "Updated Question",
            DisplayOrder = 1,
            Answers =
            [
                new UpsertAnswerRequest { Text = "New A", IsCorrect = true },
                new UpsertAnswerRequest { Text = "New B", IsCorrect = false },
                new UpsertAnswerRequest { Text = "New C", IsCorrect = false }
            ]
        };

        var result = await _svc.UpdateQuestionAsync(questionId, request);

        result.Should().NotBeNull();
        result!.Text.Should().Be("Updated Question");
        result.Answers.Should().HaveCount(3);
    }

    [Fact]
    public async Task DeleteQuestionAsync_NonExistent_ReturnsFalse()
    {
        var result = await _svc.DeleteQuestionAsync(9999);

        result.Should().BeFalse();
    }
}
