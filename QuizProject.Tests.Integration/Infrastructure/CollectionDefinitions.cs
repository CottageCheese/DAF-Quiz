namespace QuizProject.Tests.Integration.Infrastructure;

[CollectionDefinition("AuthTests")]
public class AuthTestsCollection : ICollectionFixture<CustomWebApplicationFactory> { }

[CollectionDefinition("QuizTests")]
public class QuizTestsCollection : ICollectionFixture<CustomWebApplicationFactory> { }

[CollectionDefinition("AdminTests")]
public class AdminTestsCollection : ICollectionFixture<CustomWebApplicationFactory> { }

[CollectionDefinition("LeaderboardTests")]
public class LeaderboardTestsCollection : ICollectionFixture<CustomWebApplicationFactory> { }
