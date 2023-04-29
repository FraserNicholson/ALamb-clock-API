﻿using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Shared.Contracts;
using Shared.Models;
using Shared.Options;

namespace Shared.Clients
{
    public interface IDbClient
    {
        Task SaveCricketMatches(CricketDataMatchesResponse matches);
        Task<MatchesDbModel> GetMostRecentlyStoredCricketMatches();
        Task DeleteExpiredCricketMatches();
    }
    
    public class MongoDbClient : IDbClient
    {
        private readonly MongoDbOptions _dbOptions;
        private readonly IMongoClient _mongoClient;
        private readonly IMongoDatabase _database;
        private const string MatchesCollectionName = "matches";

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
    }
}