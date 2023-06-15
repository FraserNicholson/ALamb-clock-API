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
    private IMongoCollection<MatchDbModel> _matchesCollection;
    private IMongoCollection<NotificationDbModel> _notificationCollection;

    [SetUp]
    public void SetUp()
    {
        _dbOptions = Options.Create(new MongoDbOptions { ConnectionString = "test", DatabaseName = "test" });
        _mongoClient = Mock.Of<IMongoClient>();
        _database = Mock.Of<IMongoDatabase>();
        _matchesCollection = Mock.Of<IMongoCollection<MatchDbModel>>();
        _notificationCollection = Mock.Of<IMongoCollection<NotificationDbModel>>();

        Mock.Get(_mongoClient).Setup(c => c.GetDatabase(It.IsAny<string>(), It.IsAny<MongoDatabaseSettings>()))
            .Returns(_database);
        Mock.Get(_database).Setup(d => d.GetCollection<MatchDbModel>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
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
            }
        };

        var sut = CreateSut();

        await sut.SaveCricketMatches(cricketMatchToSave);
        
        Mock.Get(_matchesCollection)
            .Verify(c => c.InsertManyAsync(
                It.IsAny<IEnumerable<MatchDbModel>>(), It.IsAny<InsertManyOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetMostRecentlyStoredCricketMatches_GivenMatchesHaveBeenStoredToday_ReturnsTodaysMatches()
    {
        var savedCricketMatches = new MatchDbModel[]
        {
            new ()
            {
                MatchId = "match-id",
                DateStored = DateTime.Today.ToString(CultureInfo.InvariantCulture)
            },
            new ()
            {
                MatchId = "match-id-2",
                DateStored = DateTime.Today.ToString(CultureInfo.InvariantCulture)
            }
        };
        
        var mockCursor = new Mock<IAsyncCursor<MatchDbModel>>();
        
        mockCursor.Setup(c => c.Current).Returns(savedCricketMatches);
        mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(true))
            .Returns(Task.FromResult(false));
        
        Mock.Get(_matchesCollection)
            .Setup(c => c.FindAsync(It.IsAny<FilterDefinition<MatchDbModel>>(), It.IsAny<FindOptions<MatchDbModel, MatchDbModel>>(), default))
            .ReturnsAsync(mockCursor.Object);

        var sut = CreateSut();

        var matches = await sut.GetAllMatches();
        
        Mock.Get(_matchesCollection)
            .Verify(c => c.FindAsync(It.IsAny<FilterDefinition<MatchDbModel>>(),
                It.IsAny<FindOptions<MatchDbModel, MatchDbModel>>(), It.IsAny<CancellationToken>()), Times.Once);
        matches.Should().BeEquivalentTo(savedCricketMatches);
    }
}