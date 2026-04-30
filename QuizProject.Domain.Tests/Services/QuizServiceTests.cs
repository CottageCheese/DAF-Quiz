using QuizProject.Contracts;
using QuizProject.Domain.Models.Domain;
using QuizProject.Domain.Services;
using QuizProject.Domain.Tests.Infrastructure;

namespace QuizProject.Domain.Tests.Services;

public class QuizServiceTests : DomainTestBase
{
    private readonly QuizService _svc;

    public QuizServiceTests()
    {
        _svc = new QuizService(QuizRepo, QuestionRepo, AttemptRepo, AttemptAnswerRepo, Cache);
    }

    [Fact]
    public async Task GetActiveQuizzesAsync_ReturnsPublished_ExcludesDraft()
    {
        var seed = await DomainTestSeeder.SeedAsync(Db);

        var result = await _svc.GetActiveQuizzesAsync();

        result.Should().ContainSingle(q => q.Title == seed.PublishedQuiz.Title);
        result.Should().NotContain(q => q.Title == seed.DraftQuiz.Title);
    }

    [Fact]
    public async Task GetActiveQuizzesAsync_ExcludesFuturePublishedAt()
    {
        Db.Quizzes.Add(new Quiz
        {
            Title = "Future Quiz",
            CreatedByUserId = "u1",
            CreatedByEmail = "u@test.local",
            CreatedAt = DateTime.UtcNow,
            PublishedAt = DateTime.UtcNow.AddDays(1)
        });
        await Db.SaveChangesAsync();

        var result = await _svc.GetActiveQuizzesAsync();

        result.Should().NotContain(q => q.Title == "Future Quiz");
    }

    [Fact]
    public async Task StartAttemptAsync_Published_ReturnsViewModel()
    {
        var seed = await DomainTestSeeder.SeedAsync(Db);

        var result = await _svc.StartAttemptAsync(seed.PublishedQuiz.Id, seed.User.Id);

        result.Should().NotBeNull();
        result!.QuizTitle.Should().Be(seed.PublishedQuiz.Title);
        result.Questions.Should().HaveCount(2);
    }

    [Fact]
    public async Task StartAttemptAsync_Draft_ReturnsNull()
    {
        var seed = await DomainTestSeeder.SeedAsync(Db);

        var result = await _svc.StartAttemptAsync(seed.DraftQuiz.Id, seed.User.Id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task SubmitAttemptAsync_AllCorrect_ScoreEqualsTotal()
    {
        var seed = await DomainTestSeeder.SeedAsync(Db);
        var takeVm = await _svc.StartAttemptAsync(seed.PublishedQuiz.Id, seed.User.Id);
        var submission = new SubmitQuizViewModel
        {
            AttemptId = takeVm!.AttemptId,
            Selections = seed.QuestionIds
                .Zip(seed.CorrectAnswerIds, (q, a) => new QuestionAnswerSelection { QuestionId = q, SelectedAnswerId = a })
                .ToList()
        };

        var result = await _svc.SubmitAttemptAsync(submission, seed.User.Id);

        result.Should().NotBeNull();
        result!.Score.Should().Be(2);
        result.TotalQuestions.Should().Be(2);
    }

    [Fact]
    public async Task SubmitAttemptAsync_AllWrong_ScoreIsZero()
    {
        var seed = await DomainTestSeeder.SeedAsync(Db);
        var takeVm = await _svc.StartAttemptAsync(seed.PublishedQuiz.Id, seed.User.Id);
        var submission = new SubmitQuizViewModel
        {
            AttemptId = takeVm!.AttemptId,
            Selections = seed.QuestionIds
                .Zip(seed.WrongAnswerIds, (q, a) => new QuestionAnswerSelection { QuestionId = q, SelectedAnswerId = a })
                .ToList()
        };

        var result = await _svc.SubmitAttemptAsync(submission, seed.User.Id);

        result!.Score.Should().Be(0);
    }

    [Fact]
    public async Task GetResultAsync_CompletedAttempt_ReturnsResult()
    {
        var seed = await DomainTestSeeder.SeedAsync(Db);
        var takeVm = await _svc.StartAttemptAsync(seed.PublishedQuiz.Id, seed.User.Id);
        var submission = new SubmitQuizViewModel
        {
            AttemptId = takeVm!.AttemptId,
            Selections = seed.QuestionIds
                .Zip(seed.CorrectAnswerIds, (q, a) => new QuestionAnswerSelection { QuestionId = q, SelectedAnswerId = a })
                .ToList()
        };
        await _svc.SubmitAttemptAsync(submission, seed.User.Id);

        var result = await _svc.GetResultAsync(takeVm.AttemptId, seed.User.Id);

        result.Should().NotBeNull();
        result!.AttemptId.Should().Be(takeVm.AttemptId);
    }

    [Fact]
    public async Task GetResultAsync_WrongUser_ReturnsNull()
    {
        var seed = await DomainTestSeeder.SeedAsync(Db);
        var takeVm = await _svc.StartAttemptAsync(seed.PublishedQuiz.Id, seed.User.Id);
        var submission = new SubmitQuizViewModel
        {
            AttemptId = takeVm!.AttemptId,
            Selections = seed.QuestionIds
                .Zip(seed.CorrectAnswerIds, (q, a) => new QuestionAnswerSelection { QuestionId = q, SelectedAnswerId = a })
                .ToList()
        };
        await _svc.SubmitAttemptAsync(submission, seed.User.Id);

        var result = await _svc.GetResultAsync(takeVm.AttemptId, "wrong-user-id");

        result.Should().BeNull();
    }

    [Theory]
    [InlineData(10, 10, "Excellent")]
    [InlineData(9, 10, "Excellent")]
    [InlineData(7, 10, "Good")]
    [InlineData(5, 10, "Pass")]
    [InlineData(4, 10, "Needs Improvement")]
    public void QuizResultViewModel_Grade_CorrectLabel(int score, int total, string expectedGrade)
    {
        var vm = new QuizResultViewModel { Score = score, TotalQuestions = total };
        vm.Grade.Should().Be(expectedGrade);
    }
}
