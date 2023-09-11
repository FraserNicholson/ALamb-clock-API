using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Shared.Contracts;
using Shared.Extensions;
using Shared.Models;
using Shared.Models.Database;
using Shared.Options;

namespace Shared.Clients;

public interface IDbClient
{
    Task SaveCricketMatches(CricketDataMatchesResponse matches);
    Task<IEnumerable<MatchDbModel>> GetAllMatches();
    Task<QueryMatchesResponse> QueryMatches(QueryMatchesRequest request);
    Task DeleteExpiredCricketMatches();
    Task<NotificationResponse> AddOrUpdateNotification(SaveNotificationDbRequest saveNotificationRequest);
    Task<IEnumerable<NotificationResponse>> GetAllNotificationsForRegistrationToken(string registrationToken);
    Task<IEnumerable<NotificationDbModel>> GetActiveNotifications();
    Task DeleteNotifications(IEnumerable<string> notificationIdsToDelete);
    Task DeleteNotification(string notificationId, string registrationToken);
}
    
public partial class MongoDbClient : IDbClient
{
    private readonly MongoDbOptions _dbOptions;
    private readonly IMongoClient _mongoClient;
    private readonly IMongoDatabase _database;
    private const string MatchesCollectionName = "matches";
    private const string NotificationsCollectionName = "notifications";
    private const int MatchesPageSize = 5;

    public MongoDbClient(IOptions<MongoDbOptions> dbOptions, IMongoClient mongoClient)
    {
        _mongoClient = mongoClient;
        _dbOptions = dbOptions.Value;

        _database = _mongoClient.GetDatabase(_dbOptions.DatabaseName);
    }
    
    public async Task SaveCricketMatches(CricketDataMatchesResponse matches)
    {
        var teamNameRegex = new Regex(@"\[.*\]");

        var matchesToStore = matches.Data
            .Where(m => m.MatchStatus != ResponseMatchStatus.Result)
            .Select(m => new MatchDbModel
            {
                Id = Guid.NewGuid().ToString(),
                MatchId = m.Id,
                DateStored = DateOnly.FromDateTime(DateTime.Today),
                Status = m.Status,
                MatchStatus = (MatchStatus)m.MatchStatus,
                Team1 = teamNameRegex.Replace(m.Team1, string.Empty).TrimEnd(),
                Team2 = teamNameRegex.Replace(m.Team2, string.Empty).TrimEnd(),
                MatchType = m.MatchType,
                DateTimeGmt = m.DateTimeGmt
            });

        var matchesCollection = _database.GetCollection<MatchDbModel>(MatchesCollectionName);
        await matchesCollection.InsertManyAsync(matchesToStore);
    }

    public async Task<IEnumerable<MatchDbModel>> GetAllMatches()
    {
        var matchesCollection = _database.GetCollection<MatchDbModel>(MatchesCollectionName);

        var filter =
            Builders<MatchDbModel>.Filter.Empty;

        return await (await matchesCollection.FindAsync<MatchDbModel>(filter)).ToListAsync();
    }

    public async Task<QueryMatchesResponse> QueryMatches(QueryMatchesRequest request)
    {
        var matchTypeFilter = BuildMatchTypeFilter(request);
        var searchTermFilter = BuildTeamSearchFilter(request);

        var combinedFilter = matchTypeFilter & searchTermFilter;

        var matchesCollection = _database.GetCollection<MatchDbModel>(MatchesCollectionName);

        var totalMatchesCount = (int)await matchesCollection.CountDocumentsAsync(combinedFilter);

        var matchesFromDb = await matchesCollection.Find(combinedFilter)
            .Skip((request.PageNumber - 1) * MatchesPageSize)
            .Limit(MatchesPageSize)
            .ToListAsync();

        var matchesResponse = new QueryMatchesResponse
        {
            Count = totalMatchesCount,
            Matches = matchesFromDb.Select(m => new MatchesResponse
            {
                Id = m.MatchId,
                Status = m.Status,
                MatchStatus = m.MatchStatus.ToString(),
                Team1 = m.Team1,
                Team2 = m.Team2,
                MatchType = m.MatchType,
                DateTimeGmt = m.DateTimeGmt
            }).ToArray(),
            CurrentPageCount = matchesFromDb.Count
        };

        return matchesResponse;
    }

    public async Task DeleteExpiredCricketMatches()
    {
        var matchesCollection = _database.GetCollection<MatchDbModel>(MatchesCollectionName);
        var dateToday = DateOnly.FromDateTime(DateTime.Today);
        
        var yesterdayFilter =
            Builders<MatchDbModel>.Filter.Lt("DateStored", dateToday);
        
        var todayFilter =
            Builders<MatchDbModel>.Filter.Eq("DateStored", dateToday);

        var numberOfMatchesStoredToday = (int)await matchesCollection.CountDocumentsAsync(todayFilter);

        if (numberOfMatchesStoredToday > 0)
        {
            await matchesCollection.DeleteManyAsync(yesterdayFilter);
        }
    }

    public async Task<IEnumerable<NotificationResponse>> GetAllNotificationsForRegistrationToken(string registrationToken)
    {
        var notificationsCollection = _database.GetCollection<NotificationDbModel>(NotificationsCollectionName);

        var filter = Builders<NotificationDbModel>.Filter.Eq("RegistrationTokens", registrationToken);

        return (await notificationsCollection.FindAsync(filter)).ToList().ToResponse();
    }

    public async Task<IEnumerable<NotificationDbModel>> GetActiveNotifications()
    {
        var notificationsCollection = _database.GetCollection<NotificationDbModel>(NotificationsCollectionName);

        var filter = Builders<NotificationDbModel>.Filter.Lte("DateTimeGmt",
            DateTime.UtcNow.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz", CultureInfo.InvariantCulture));

        return (await notificationsCollection.FindAsync(filter)).ToList();
    }

    public async Task DeleteNotifications(IEnumerable<string> notificationIdsToDelete)
    {
        var notificationsCollection = _database.GetCollection<NotificationDbModel>(NotificationsCollectionName);

        var filter = Builders<NotificationDbModel>.Filter.In("_id", notificationIdsToDelete);
        
        await notificationsCollection.DeleteManyAsync(filter);
    }

    public async Task DeleteNotification(string notificationId, string registrationToken)
    {
        var notificationsCollection = _database.GetCollection<NotificationDbModel>(NotificationsCollectionName);

        var idFilter = Builders<NotificationDbModel>.Filter.Eq("Id", notificationId);
        var registrationIdFilter = Builders<NotificationDbModel>.Filter.AnyStringIn(n => n.RegistrationTokens,
            registrationToken);

        var notification = await (await notificationsCollection.FindAsync(idFilter & registrationIdFilter))
            .SingleOrDefaultAsync();

        if (notification?.RegistrationTokens.Any(t => t != registrationToken) == true)
        {
            await RemoveRegistrationTokenFromNotification(notificationsCollection, notification, registrationToken);
            return;
        }

        await notificationsCollection.DeleteOneAsync(idFilter & registrationIdFilter);
    }

    public async Task<NotificationResponse> AddOrUpdateNotification(SaveNotificationDbRequest saveNotificationRequest)
    {
        // If we are updating an existing notification, we need to remove the old notification to prevent duplicates
        if (saveNotificationRequest.NotificationId != null)
        {
            await DeleteNotification(saveNotificationRequest.NotificationId, saveNotificationRequest.RegistrationToken);
        }
        
        var notificationsCollection = _database.GetCollection<NotificationDbModel>(NotificationsCollectionName);

        var idFilter = Builders<NotificationDbModel>.Filter.Eq("MatchId",
            saveNotificationRequest.MatchId);
        var notificationTypeFiler = Builders<NotificationDbModel>.Filter.Eq("NotificationType",
            saveNotificationRequest.NotificationType);
        var teamInQuestionFilter = Builders<NotificationDbModel>.Filter.Eq("TeamInQuestion",
            saveNotificationRequest.TeamInQuestion);
        var numberOfWicketsFilter = Builders<NotificationDbModel>.Filter.Eq("NumberOfWickets",
            saveNotificationRequest.NumberOfWickets);

        var existingNotifications = (await notificationsCollection
                .FindAsync(idFilter & notificationTypeFiler & teamInQuestionFilter & numberOfWicketsFilter))
            .ToList();

        if (!existingNotifications.Any())
        {
            return await AddNewNotification(notificationsCollection, saveNotificationRequest);
        }

        return await UpdateExistingNotification(
            notificationsCollection,
            existingNotifications,
            saveNotificationRequest);
    }

    private async Task<NotificationResponse> AddNewNotification(
        IMongoCollection<NotificationDbModel> collection,
        SaveNotificationDbRequest saveNotificationRequest)
    {
        var matchesCollection = _database.GetCollection<MatchDbModel>(MatchesCollectionName);

        var filter = Builders<MatchDbModel>.Filter.Eq("MatchId", saveNotificationRequest.MatchId);

        var match = await (await matchesCollection.FindAsync(filter)).SingleOrDefaultAsync();
        
        var notificationDbModel = new NotificationDbModel
        {
            Id = saveNotificationRequest.NotificationId ?? Guid.NewGuid().ToString(),
            MatchId = saveNotificationRequest.MatchId,
            Team1 = match.Team1,
            Team2 = match.Team2,
            DateTimeGmt = match.DateTimeGmt,
            NotificationType = saveNotificationRequest.NotificationType,
            TeamInQuestion = saveNotificationRequest.TeamInQuestion,
            NumberOfWickets = saveNotificationRequest.NumberOfWickets,
            RegistrationTokens = new List<string> { saveNotificationRequest.RegistrationToken },
        };

        await collection.InsertOneAsync(notificationDbModel);

        return notificationDbModel.ToResponse();
    }

    private async Task<NotificationResponse> UpdateExistingNotification(
        IMongoCollection<NotificationDbModel> collection,
        IEnumerable<NotificationDbModel> existingNotifications,
        SaveNotificationDbRequest saveNotificationRequest)
    {
        // Firebase Cloud Messaging only supports sending batch notifications for 500 registration tokens
        // at once. So we limit the number of registration tokens to 500 per record
        var notificationToUpdate = existingNotifications
            .FirstOrDefault(n => n.RegistrationTokens.Count < 500);

        if (notificationToUpdate is null)
        {
            return await AddNewNotification(collection, saveNotificationRequest);
        }

        var updatedRegistrationTokens = notificationToUpdate.RegistrationTokens;
        updatedRegistrationTokens
            .Add(saveNotificationRequest.RegistrationToken);

        var filter = Builders<NotificationDbModel>.Filter.Eq("Id", notificationToUpdate.Id);
        var update = Builders<NotificationDbModel>.Update
            .Set(n => n.RegistrationTokens, updatedRegistrationTokens);

        await collection.UpdateOneAsync(filter, update);

        return notificationToUpdate.ToResponse();
    }

    private static async Task RemoveRegistrationTokenFromNotification(
        IMongoCollection<NotificationDbModel> collection,
        NotificationDbModel notification,
        string registrationTokenToRemove)
    {
        var updatedRegistrationTokens = notification.RegistrationTokens;
        updatedRegistrationTokens.RemoveAll(t => t == registrationTokenToRemove);
        
        var filter = Builders<NotificationDbModel>.Filter.Eq("Id", notification.Id);
        var update = Builders<NotificationDbModel>.Update
            .Set(n => n.RegistrationTokens, updatedRegistrationTokens);

        await collection.UpdateOneAsync(filter, update);
    }

    private static FilterDefinition<MatchDbModel> BuildMatchTypeFilter(QueryMatchesRequest request)
    {
        return string.IsNullOrWhiteSpace(request.MatchType)
            ? Builders<MatchDbModel>.Filter.Empty
            : Builders<MatchDbModel>.Filter.Eq("MatchType", request.MatchType == "county" ? string.Empty : request.MatchType);
    }

    private static FilterDefinition<MatchDbModel> BuildTeamSearchFilter(QueryMatchesRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TeamSearchTerm))
        {
            return Builders<MatchDbModel>.Filter.Empty;
        }

        // The "i" ensures we get case insensitive matches
        var filter = Builders<MatchDbModel>.Filter.Regex("Team1", new BsonRegularExpression(request.TeamSearchTerm, "i"))
                     | Builders<MatchDbModel>.Filter.Regex("Team2", new BsonRegularExpression(request.TeamSearchTerm, "i"));

        return filter;
    }
}