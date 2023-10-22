using FluentAssertions;
using Moq;
using NUnit.Framework;
using Shared.Clients;
using Shared.Contracts;
using Shared.Messaging;
using Shared.Models.Database;
using Functions.Services;

namespace UnitTests.Services;

[TestFixture]
public class CheckNotificationsServiceTests
{
    private IDbClient _dbClient;
    private ICricketDataApiClient _cricketDataApiClient;
    private INotificationProducer _notificationProducer;

    private static CricketDataCurrentMatchesResponse[] _noNotificationsSatisfiedTestCases =
        TestData.NoNotificationsSatisfiedTestCases;

    private static CricketDataCurrentMatchesResponse[] _oneNotificationsSatisfiedTestCases =
        TestData.OneNotificationsSatisfiedTestCases;
    
    private static CricketDataCurrentMatchesResponse[] _bothNotificationsSatisfiedTestCases =
        TestData.BothNotificationsSatisfiedTestCases;

    [SetUp]
    public void SetUp()
    {
        _dbClient = Mock.Of<IDbClient>();
        _cricketDataApiClient = Mock.Of<ICricketDataApiClient>();
        _notificationProducer = Mock.Of<INotificationProducer>();
    }
    
    private CheckNotificationsService CreateSut()
    {
        return new CheckNotificationsService(
            _dbClient,
            _cricketDataApiClient,
            _notificationProducer);
    }

    [Test]
    public async Task CheckAndSendNotifications_WithNoActiveNotifications_ShouldNotSendNotifications()
    {
        Mock.Get(_dbClient).Setup(c => c.GetActiveNotifications())
            .ReturnsAsync(Enumerable.Empty<NotificationDbModel>());

        var sut = CreateSut();

        var notificationsSent = await sut.CheckAndSendNotifications();
        
        Mock.Get(_notificationProducer)
            .Verify(p => p.SendNotifications(It.IsAny<IEnumerable<NotificationDbModel>>()), Times.Never);
        Mock.Get(_cricketDataApiClient)
            .Verify(p => p.GetCurrentMatches(0), Times.Never);
        Mock.Get(_dbClient)
            .Verify(p => p.DeleteNotifications(It.IsAny<IEnumerable<string>>()), Times.Never);
        
        notificationsSent.Should().Be(0);
    }
    
    [TestCaseSource(nameof(_noNotificationsSatisfiedTestCases))]
    public async Task CheckAndSendNotifications_WithNoSatisfiedNotifications_ShouldNotSendNotifications(
        CricketDataCurrentMatchesResponse apiResponse)
    {
        Mock.Get(_dbClient).Setup(c => c.GetActiveNotifications())
            .ReturnsAsync(TestData.MockActiveNotifications);
        Mock.Get(_cricketDataApiClient).Setup(c => c.GetCurrentMatches(0))
            .ReturnsAsync(apiResponse);

        var sut = CreateSut();

        var notificationsSent = await sut.CheckAndSendNotifications();
        
        Mock.Get(_notificationProducer)
            .Verify(p => p.SendNotifications(It.IsAny<IEnumerable<NotificationDbModel>>()), Times.Never);
        Mock.Get(_dbClient)
            .Verify(p => p.DeleteNotifications(It.IsAny<IEnumerable<string>>()), Times.Never);

        notificationsSent.Should().Be(0);
    }
    
    [TestCaseSource(nameof(_oneNotificationsSatisfiedTestCases))]
    public async Task CheckAndSendNotifications_WithOneSatisfiedNotification_ShouldSendOneNotifications(
        CricketDataCurrentMatchesResponse apiResponse)
    {
        Mock.Get(_dbClient).Setup(c => c.GetActiveNotifications())
            .ReturnsAsync(TestData.MockActiveNotifications);
        Mock.Get(_cricketDataApiClient).Setup(c => c.GetCurrentMatches(0))
            .ReturnsAsync(apiResponse);

        var sut = CreateSut();

        var notificationsSent = await sut.CheckAndSendNotifications();
        
        Mock.Get(_notificationProducer)
            .Verify(p => p.SendNotifications(It.IsAny<IEnumerable<NotificationDbModel>>()), Times.Once);
        Mock.Get(_dbClient)
            .Verify(p => p.DeleteNotifications(It.IsAny<IEnumerable<string>>()), Times.Once);

        notificationsSent.Should().Be(1);
    }
    
    [TestCaseSource(nameof(_bothNotificationsSatisfiedTestCases))]
    public async Task CheckAndSendNotifications_WithAllSatisfiedNotification_ShouldSendAllNotifications(
        CricketDataCurrentMatchesResponse apiResponse)
    {
        Mock.Get(_dbClient).Setup(c => c.GetActiveNotifications())
            .ReturnsAsync(TestData.MockActiveNotifications);
        Mock.Get(_cricketDataApiClient).Setup(c => c.GetCurrentMatches(0))
            .ReturnsAsync(apiResponse);

        var sut = CreateSut();

        var notificationsSent = await sut.CheckAndSendNotifications();
        
        Mock.Get(_notificationProducer)
            .Verify(p => p.SendNotifications(It.IsAny<IEnumerable<NotificationDbModel>>()), Times.Once);
        Mock.Get(_dbClient)
            .Verify(p => p.DeleteNotifications(It.IsAny<IEnumerable<string>>()), Times.Once);

        notificationsSent.Should().Be(2);
    }

    [Test]
    public async Task CheckAndSendNotifications_WithAllExpireNotifications_ShouldDeleteAllNotifications()
    {
        var apiResponse = new CricketDataCurrentMatchesResponse()
        {
            Data = new CricketDataCurrentMatch[]
            {
                new()
                {
                    Id = "match1",
                    MatchEnded = true,
                }
            }
        };

        var apiResponse2 = new CricketDataCurrentMatchesResponse()
        {
            Data = new CricketDataCurrentMatch[]
    {
                new()
                {
                    Id = "match1000",
                    MatchEnded = true,
                }
    }
        };

        Mock.Get(_dbClient).Setup(c => c.GetActiveNotifications())
            .ReturnsAsync(TestData.MockActiveNotifications);
        Mock.Get(_cricketDataApiClient).Setup(c => c.GetCurrentMatches(0))
            .ReturnsAsync(apiResponse);
        Mock.Get(_cricketDataApiClient).Setup(c => c.GetCurrentMatches(25))
            .ReturnsAsync(apiResponse2);

        var sut = CreateSut();

        var notificationsSent = await sut.CheckAndSendNotifications();

        var notificationsExpectedToBeDeleted = new List<string>() { "notification1" };

        Mock.Get(_notificationProducer)
            .Verify(p => p.SendNotifications(It.IsAny<IEnumerable<NotificationDbModel>>()), Times.Never);
        Mock.Get(_dbClient)
            .Verify(p => p.DeleteNotifications(notificationsExpectedToBeDeleted), Times.Once);

        notificationsSent.Should().Be(0);
    }

    [TestCaseSource(nameof(_noNotificationsSatisfiedTestCases))]
    public async Task CheckAndSendNotifications_WhenRequiredMatchesAreNotInFirstPageOfResponse_FetchesSecondPage(
    CricketDataCurrentMatchesResponse apiResponse)
    {
        Mock.Get(_dbClient).Setup(c => c.GetActiveNotifications())
            .ReturnsAsync(TestData.MockActiveNotifications);

        Mock.Get(_cricketDataApiClient).Setup(c => c.GetCurrentMatches(0))
            .ReturnsAsync(new CricketDataCurrentMatchesResponse
            {
                Data = new[]
                {
                    new CricketDataCurrentMatch { Id = "random match"}
                } 
            });

        Mock.Get(_cricketDataApiClient).Setup(c => c.GetCurrentMatches(25))
            .ReturnsAsync(apiResponse);

        var sut = CreateSut();

        var notificationsSent = await sut.CheckAndSendNotifications();

        Mock.Get(_cricketDataApiClient)
            .Verify(c => c.GetCurrentMatches(0), Times.Once);
        Mock.Get(_cricketDataApiClient)
            .Verify(c => c.GetCurrentMatches(25), Times.Once);
        Mock.Get(_notificationProducer)
            .Verify(p => p.SendNotifications(It.IsAny<IEnumerable<NotificationDbModel>>()), Times.Never);
        Mock.Get(_dbClient)
            .Verify(p => p.DeleteNotifications(It.IsAny<IEnumerable<string>>()), Times.Never);

        notificationsSent.Should().Be(0);
    }
}