using Worker.Services;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Clients;
using Shared.Messaging;
using Shared.Options;
using Worker;

internal class Program
{
    private static async Task Main(string[] args)
    {
        IHost host = Host.CreateDefaultBuilder(args)
        .ConfigureServices(services =>
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

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

            var firebaseServiceAccountConfiguration = configuration.GetSection("Firebase:ServiceAccount").Get<Dictionary<string, object>>();
            var firebaseServiceAccountJson = JsonConvert.SerializeObject(firebaseServiceAccountConfiguration);

            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromJson(firebaseServiceAccountJson),
                    ProjectId = firebaseConfig.Get<FirebaseOptions>()?.ProjectId,
                });
            }

            JsonConvert.DefaultSettings = () =>
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new StringEnumConverter());
                return settings;
            };

            services.AddHostedService<WorkerService>();
            services.AddSingleton<CheckNotificationsService>();
            services.AddSingleton<INotificationProducer, FirebaseNotificationProducer>();
        })
        .ConfigureLogging(services =>
        {
            services.AddConsole();
        })
        .Build();

        await host.StartAsync();
    }
}