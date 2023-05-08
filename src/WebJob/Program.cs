﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Clients;
using Shared.Options;

var builder = new HostBuilder();

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile("appsettings.local.json", optional: true)
    .AddEnvironmentVariables()
    .Build();
            
builder.ConfigureServices(services =>
{
    var cricketApiConfig = configuration.GetSection("CricketDataApi");
    var mongoDbConfig = configuration.GetSection("MongoDb");

    services.Configure<CricketDataApiOptions>(cricketApiConfig);
    services.AddHttpClient<ICricketDataApiClient, CricketDataApiClient>(c =>
    {
        c.BaseAddress = cricketApiConfig.Get<CricketDataApiOptions>()?.BaseUri;
    });

    services.Configure<MongoDbOptions>(mongoDbConfig);
    services.AddSingleton<IMongoClient>(_ =>
        new MongoClient(mongoDbConfig.Get<MongoDbOptions>()?.ConnectionString)
    );

    services.AddSingleton<IDbClient, MongoDbClient>();
});
            
JsonConvert.DefaultSettings = (() =>
{
    var settings = new JsonSerializerSettings();
    settings.Converters.Add(new StringEnumConverter());
    return settings;
});
            
builder.UseEnvironment(EnvironmentName.Development);
builder.ConfigureLogging((context, b) =>
{
    b.AddConsole();
});
            
builder.ConfigureWebJobs(b =>
{
    b.AddAzureStorageQueues();
    b.AddAzureStorageBlobs();
    b.AddTimers();
});
            
var host = builder.Build();
using (host)
{
    await host.RunAsync();
}