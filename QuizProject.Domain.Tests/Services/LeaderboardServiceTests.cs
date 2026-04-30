using QuizProject.Domain.Models.Domain;
using QuizProject.Domain.Services;
using QuizProject.Domain.Tests.Infrastructure;

namespace QuizProject.Domain.Tests.Services;

public class LeaderboardServiceTests : DomainTestBase
{
    private readonly LeaderboardService _svc;

    public LeaderboardServiceTests()
    {
        _svc = new LeaderboardService(AttemptRepo);
    }

    [Fact]
    public async Task GetTopQuizzesAsync_RankedByCompletedAttemptCount()
    {
        var seed = await DomainTestSeeder.SeedAsync(Db);

        // 2 completed attempts on published quiz
        for (var i = 0; i < 2; i++)
        {
            Db.QuizAttempts.Add(new QuizAttempt
            {
                UserId = seed.User.Id,
                QuizId = seed.PublishedQuiz.Id,
                StartedAt = DateTime.UtcNow.AddMinutes(-10),
                CompletedAt = DateTime.UtcNow,
                Score = 1,
                TotalQuestions = 2
            });
        }
        await Db.SaveChangesAsync();

        var result = await _svc.GetTopQuizzesAsync(10);

        result.Should().ContainSingle();
        result[0].QuizTitle.Should().Be(seed.PublishedQuiz.Title);
        result[0].AttemptCount.Should().Be(2);
        result[0].Rank.Should().Be(1);
    }

    [Fact]
    public async Task GetTopQuizzesAsync_ExcludesIncompleteAttempts()
    {
        var seed = await DomainTestSeeder.SeedAsync(Db);

        // Incomplete attempt — no CompletedAt
        Db.QuizAttempts.Add(new QuizAttempt
        {
            UserId = seed.User.Id,
            QuizId = seed.PublishedQuiz.Id,
            StartedAt = DateTime.UtcNow,
            Score = 0,
            TotalQuestions = 2
        });
        await Db.SaveChangesAsync();

        var result = await _svc.GetTopQuizzesAsync(10);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTopUsersAsync_RankedByBestScorePercent()
    {
        var seed = await DomainTestSeeder.SeedAsync(Db);

        // Second user
        var user2 = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "user2@test.local",
            NormalizedUserName = "USER2@TEST.LOCAL",
            Email = "user2@test.local",
            NormalizedEmail = "USER2@TEST.LOCAL",
            DisplayName = "User2",
            SecurityStamp = Guid.NewGuid().ToString()
        };
        Db.Users.Add(user2);

        // User1: 2/2 = 100%
        Db.QuizAttempts.Add(new QuizAttempt
        {
            UserId = seed.User.Id,
            QuizId = seed.PublishedQuiz.Id,
            StartedAt = DateTime.UtcNow.AddMinutes(-5),
            CompletedAt = DateTime.UtcNow,
            Score = 2,
            TotalQuestions = 2
        });
        // User2: 1/2 = 50%
        Db.QuizAttempts.Add(new QuizAttempt
        {
            UserId = user2.Id,
            QuizId = seed.PublishedQuiz.Id,
            StartedAt = DateTime.UtcNow.AddMinutes(-5),
            CompletedAt = DateTime.UtcNow,
            Score = 1,
            TotalQuestions = 2
        });
        await Db.SaveChangesAsync();

        var result = await _svc.GetTopUsersAsync(10);

        result.Should().HaveCount(2);
        result[0].UserName.Should().Be(seed.User.DisplayName);
        result[0].BestScorePercent.Should().Be(100.0);
        result[1].BestScorePercent.Should().Be(50.0);
    }

    [Fact]
    public async Task GetTopUsersAsync_BestScorePercentRoundedToOneDecimal()
    {
        var seed = await DomainTestSeeder.SeedAsync(Db);

        // 1/3 = 33.333...% → rounds to 33.3
        Db.QuizAttempts.Add(new QuizAttempt
        {
            UserId = seed.User.Id,
            QuizId = seed.PublishedQuiz.Id,
            StartedAt = DateTime.UtcNow.AddMinutes(-5),
            CompletedAt = DateTime.UtcNow,
            Score = 1,
            TotalQuestions = 3
        });
        await Db.SaveChangesAsync();

        var result = await _svc.GetTopUsersAsync(10);

        result[0].BestScorePercent.Should().Be(33.3);
    }
}
