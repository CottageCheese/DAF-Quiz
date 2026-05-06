using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using QuizProject.Contracts;
using QuizProject.Tests.Integration.Infrastructure;

namespace QuizProject.Tests.Integration.Leaderboard;

[Collection("LeaderboardTests")]
public class LeaderboardTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetTopQuizzes_Returns200WithRankedList()
    {
        var response = await Client.GetAsync("/api/leaderboard/top-quizzes");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var quizzes = await response.Content.ReadFromJsonAsync<List<TopQuizViewModel>>();
        quizzes.Should().NotBeNull();
        if (quizzes!.Count > 0)
        {
            quizzes[0].Rank.Should().Be(1);
            quizzes[0].QuizTitle.Should().NotBeNullOrWhiteSpace();
            quizzes[0].AttemptCount.Should().BeGreaterThanOrEqualTo(0);
        }
    }

    [Fact]
    public async Task GetTopQuizzes_Anonymous_Returns200()
    {
        // No auth header — leaderboard is public
        var response = await Client.GetAsync("/api/leaderboard/top-quizzes");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTopQuizzes_CountDefault_Returns10OrFewer()
    {
        var response = await Client.GetAsync("/api/leaderboard/top-quizzes");
        var quizzes = await response.Content.ReadFromJsonAsync<List<TopQuizViewModel>>();
        quizzes!.Count.Should().BeLessThanOrEqualTo(10);
    }

    [Fact]
    public async Task GetTopQuizzes_CountClamped_MaxIs50()
    {
        var response = await Client.GetAsync("/api/leaderboard/top-quizzes?count=100");
        var quizzes = await response.Content.ReadFromJsonAsync<List<TopQuizViewModel>>();
        quizzes!.Count.Should().BeLessThanOrEqualTo(50);
    }

    [Fact]
    public async Task GetTopUsers_Returns200WithRankedList()
    {
        var response = await Client.GetAsync("/api/leaderboard/top-users");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<List<TopUserViewModel>>();
        users.Should().NotBeNull();
        if (users!.Count > 0)
        {
            users[0].Rank.Should().Be(1);
            users[0].UserName.Should().NotBeNullOrWhiteSpace();
            users[0].BestScorePercent.Should().BeInRange(0, 100);
        }
    }

    [Fact]
    public async Task GetTopUsers_Anonymous_Returns200()
    {
        var response = await Client.GetAsync("/api/leaderboard/top-users");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
