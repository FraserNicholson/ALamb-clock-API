using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Clients;
using Shared.Options;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using WebJob.Services;
using Shared.Messaging;

var builder = new HostBuilder();

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile("appsettings.local.json", optional: true)
    .AddJsonFile("appsettings.firebase.json")
    .AddEnvironmentVariables()
    .Build();
            
builder.ConfigureServices(services =>
{
    var cricketApiConfig = configuration.GetSection("CricketDataApi");
    var mongoDbConfig = configuration.GetSection("MongoDb");
    var firebaseConfig = configuration.GetSection("Firebase");

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

    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.FromFile("appsettings.firebase.json"),
        ProjectId = firebaseConfig.Get<FirebaseOptions>()?.ProjectId,
    });

    services.AddSingleton<ICheckNotificationsService, CheckNotificationsService>();
    services.AddSingleton<INotificationProducer, FirebaseNotificationProducer>();
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