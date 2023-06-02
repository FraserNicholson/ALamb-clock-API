using System.Globalization;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Shared.Contracts;
using Shared.Models.Database;
using Shared.Options;

namespace Shared.Clients;

public interface IDbClient
{
    Task SaveCricketMatches(CricketDataMatchesResponse matches);
    Task<MatchesDbModel> GetMostRecentlyStoredCricketMatches();
    Task DeleteExpiredCricketMatches();
    Task<NotificationDbModel> AddOrUpdateNotification(AddNotificationRequest addNotificationRequest);
    Task<IEnumerable<NotificationDbModel>> GetAllNotifications();
    Task<IEnumerable<NotificationDbModel>> GetActiveNotifications();
    Task DeleteNotifications(IEnumerable<string> notificationIdsToDelete);
}
    
public class MongoDbClient : IDbClient
{
    private readonly MongoDbOptions _dbOptions;
    private readonly IMongoClient _mongoClient;
    private readonly IMongoDatabase _database;
    private const string MatchesCollectionName = "matches";
    private const string NotificationsCollectionName = "notifications";

    public MongoDbClient(IOptions<MongoDbOptions> dbOptions, IMongoClient mongoClient)
    {
        _mongoClient = mongoClient;
        _dbOptions = dbOptions.Value;

        _database = _mongoClient.GetDatabase(_dbOptions.DatabaseName);
    }


    public async Task SaveCricketMatches(CricketDataMatchesResponse matches)
    {
        var matchesDbModel = new MatchesDbModel()
        {
            Id = Guid.NewGuid().ToString(),
            Data = matches.Data,
            DateStored = matches.DateStored
        };
            
        var matchesCollection = _database.GetCollection<MatchesDbModel>(MatchesCollectionName);
        await matchesCollection.InsertOneAsync(matchesDbModel);
    }

    public async Task<MatchesDbModel> GetMostRecentlyStoredCricketMatches()
    {
        var matchesCollection = _database.GetCollection<MatchesDbModel>(MatchesCollectionName);

        var filter =
            Builders<MatchesDbModel>.Filter.Eq("DateStored",
                DateOnly.FromDateTime(DateTime.Today).ToString());

        var matchesStoredToday =
            await (await matchesCollection.FindAsync<MatchesDbModel>(filter)).FirstOrDefaultAsync();

        if (matchesStoredToday is not null)
        {
            return matchesStoredToday;
        }

        filter = Builders<MatchesDbModel>.Filter.Empty;

        var mostRecentlyStoredMatches = await matchesCollection.Find(filter).SortByDescending(m => m.DateStored)
            .FirstOrDefaultAsync();

        return mostRecentlyStoredMatches;
    }

    public async Task DeleteExpiredCricketMatches()
    {
        var matchesCollection = _database.GetCollection<MatchesDbModel>(MatchesCollectionName);

        var filter =
            Builders<MatchesDbModel>.Filter.Lte("DateStored",
                DateOnly.FromDateTime(DateTime.Today.AddDays(-7)).ToString());

        await matchesCollection.DeleteManyAsync(filter);
    }

    public async Task<IEnumerable<NotificationDbModel>> GetAllNotifications()
    {
        var notificationsCollection = _database.GetCollection<NotificationDbModel>(NotificationsCollectionName);

        var emptyFilter = Builders<NotificationDbModel>.Filter.Empty;

        return await (await notificationsCollection.FindAsync(emptyFilter)).ToListAsync();
    }

    public async Task<IEnumerable<NotificationDbModel>> GetActiveNotifications()
    {
        var notificationsCollection = _database.GetCollection<NotificationDbModel>(NotificationsCollectionName);

        var filter = Builders<NotificationDbModel>.Filter.Lte("MatchStartsAt",
            DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));

        return await (await notificationsCollection.FindAsync(filter)).ToListAsync();
    }

    public async Task DeleteNotifications(IEnumerable<string> notificationIdsToDelete)
    {
        var notificationsCollection = _database.GetCollection<NotificationDbModel>(NotificationsCollectionName);

        var filter = Builders<NotificationDbModel>.Filter.Eq("Id", notificationIdsToDelete);

        await notificationsCollection.DeleteManyAsync(filter);
    }

    public async Task<NotificationDbModel> AddOrUpdateNotification(AddNotificationRequest addNotificationRequest)
    {
        var notificationsCollection = _database.GetCollection<NotificationDbModel>(NotificationsCollectionName);

        var idFilter = Builders<NotificationDbModel>.Filter.Eq("MatchId",
            addNotificationRequest.MatchId);
        var notificationTypeFiler = Builders<NotificationDbModel>.Filter.Eq("NotificationType",
            addNotificationRequest.NotificationType);
        var teamInQuestionFilter = Builders<NotificationDbModel>.Filter.Eq("TeamInQuestion",
            addNotificationRequest.TeamInQuestion);
        var numberOfWicketsFilter = Builders<NotificationDbModel>.Filter.Eq("NumberOfWickets",
            addNotificationRequest.NumberOfWickets);

        var existingNotifications = await (await notificationsCollection
                .FindAsync(idFilter & notificationTypeFiler & teamInQuestionFilter & numberOfWicketsFilter))
            .ToListAsync();

        if (!existingNotifications.Any())
        {
            return await AddNewNotification(notificationsCollection, addNotificationRequest);
        }

        return await UpdateExistingNotification(
            notificationsCollection,
            existingNotifications,
            addNotificationRequest);
    }

    private static async Task<NotificationDbModel> AddNewNotification(
        IMongoCollection<NotificationDbModel> collection,
        AddNotificationRequest addNotificationRequest)
    {
        var notificationDbModel = new NotificationDbModel
        {
            Id = Guid.NewGuid().ToString(),
            MatchId = addNotificationRequest.MatchId,
            MatchStartsAt = addNotificationRequest.MatchStartsAt,
            NotificationType = addNotificationRequest.NotificationType,
            TeamInQuestion = addNotificationRequest.TeamInQuestion,
            NumberOfWickets = addNotificationRequest.NumberOfWickets,
            RegistrationTokens = new List<string> { addNotificationRequest.RegistrationToken },
        };

        await collection.InsertOneAsync(notificationDbModel);

        return notificationDbModel;
    }

    private static async Task<NotificationDbModel> UpdateExistingNotification(
        IMongoCollection<NotificationDbModel> collection,
        IEnumerable<NotificationDbModel> existingNotifications,
        AddNotificationRequest addNotificationRequest)
    {
        // Firebase Cloud Messaging only supports sending batch notifications for 500 registration tokens
        // at once. So we limit the number of registration tokens to 500 per record
        var notificationToUpdate = existingNotifications
            .FirstOrDefault(n => n.RegistrationTokens.Count < 500);

        if (notificationToUpdate is null)
        {
            return await AddNewNotification(collection, addNotificationRequest);
        }

        var updatedRegistrationTokens = notificationToUpdate.RegistrationTokens;
        updatedRegistrationTokens
            .Add(addNotificationRequest.RegistrationToken);

        var filter = Builders<NotificationDbModel>.Filter.Eq("Id", notificationToUpdate.Id);
        var update = Builders<NotificationDbModel>.Update
            .Set(n => n.RegistrationTokens, updatedRegistrationTokens);

        await collection.UpdateOneAsync(filter, update);

        return notificationToUpdate;
    }
}