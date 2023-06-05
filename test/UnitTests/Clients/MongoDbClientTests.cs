using System.Globalization;
using FluentAssertions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;
using Shared.Clients;
using Shared.Contracts;
using Shared.Models.Database;
using Shared.Options;

namespace UnitTests.Clients;

[TestFixture]
public class MongoDbClientTests
{
    private IOptions<MongoDbOptions> _dbOptions;
    private IMongoClient _mongoClient;
    private IMongoDatabase _database;
    private IMongoCollection<MatchesDbModel> _matchesCollection;
    private IMongoCollection<NotificationDbModel> _notificationCollection;

    [SetUp]
    public void SetUp()
    {
        _dbOptions = Options.Create(new MongoDbOptions { ConnectionString = "test", DatabaseName = "test" });
        _mongoClient = Mock.Of<IMongoClient>();
        _database = Mock.Of<IMongoDatabase>();
        _matchesCollection = Mock.Of<IMongoCollection<MatchesDbModel>>();
        _notificationCollection = Mock.Of<IMongoCollection<NotificationDbModel>>();

        Mock.Get(_mongoClient).Setup(c => c.GetDatabase(It.IsAny<string>(), It.IsAny<MongoDatabaseSettings>()))
            .Returns(_database);
        Mock.Get(_database).Setup(d => d.GetCollection<MatchesDbModel>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
            .Returns(_matchesCollection);
        Mock.Get(_database).Setup(d => d.GetCollection<NotificationDbModel>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
            .Returns(_notificationCollection);
    }

    private MongoDbClient CreateSut()
    {
        return new MongoDbClient(_dbOptions, _mongoClient);
    }

    [Test]
    public async Task SaveCricketMatches_ShouldSaveCricketMatches()
    {
        var cricketMatchToSave = new CricketDataMatchesResponse()
        {
            Data = new CricketDataMatch[]
            {
                new()
                {
                    Id = "matchId"
                }
            },
            DateStored = "2023-06-04"
        };

        var sut = CreateSut();

        await sut.SaveCricketMatches(cricketMatchToSave);
        
        Mock.Get(_matchesCollection)
            .Verify(c => c.InsertOneAsync(
                It.IsAny<MatchesDbModel>(), It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetMostRecentlyStoredCricketMatches_GivenMatchesHaveBeenStoredToday_ReturnsTodaysMatches()
    {
        var savedCricketMatches = new MatchesDbModel
        {
            Data = Array.Empty<CricketDataMatch>(),
            DateStored = DateTime.Today.ToString(CultureInfo.InvariantCulture),
            Id = "matches-id"
        };
        
        Mock.Get(_matchesCollection)
            .Setup(c => c.FindAsync(It.IsAny<FilterDefinition<MatchesDbModel>>(), It.IsAny<FindOptions<MatchesDbModel, MatchesDbModel>>(), default))
            .ReturnsAsync(MockCursor(savedCricketMatches));

        var sut = CreateSut();

        var matches = await sut.GetMostRecentlyStoredCricketMatches();
        
        Mock.Get(_matchesCollection)
            .Verify(c => c.FindAsync(It.IsAny<FilterDefinition<MatchesDbModel>>(),
                It.IsAny<FindOptions<MatchesDbModel, MatchesDbModel>>(), It.IsAny<CancellationToken>()), Times.Once);
        matches.Should().BeEquivalentTo(savedCricketMatches);
    }

    private static IAsyncCursor<T> MockCursor<T>(params T[] items)
    {
        var mockCursor = new Mock<IAsyncCursor<T>>();

        var itemsList = new List<T>(items);
        mockCursor.SetupSequence(c => c.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(() => itemsList.Any())
            .Returns(() => false);

        mockCursor.SetupGet(c => c.Current)
            .Returns(() => itemsList);

        return mockCursor.Object;
    }
}