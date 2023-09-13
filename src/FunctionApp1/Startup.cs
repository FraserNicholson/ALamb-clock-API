using FirebaseAdmin;
using Functions.Services;
using Google.Apis.Auth.OAuth2;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Clients;
using Shared.Messaging;
using Shared.Options;
using System.Collections.Generic;
using System.Xml.Linq;

[assembly: FunctionsStartup(typeof(FunctionApp1.Startup))]

namespace FunctionApp1;
public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var context = builder.GetContext();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(context.ApplicationRootPath)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var cricketApiConfig = configuration.GetSection("CricketDataApi");
        var mongoDbConfig = configuration.GetSection("MongoDb");
        var firebaseConfig = configuration.GetSection("Firebase");

        builder.Services.Configure<CricketDataApiOptions>(cricketApiConfig);

        builder.Services.AddHttpClient<ICricketDataApiClient, CricketDataApiClient>(c =>
        {
            c.BaseAddress = cricketApiConfig.Get<CricketDataApiOptions>()?.BaseUri;
        });


        builder.Services.Configure<MongoDbOptions>(mongoDbConfig);
        builder.Services.AddSingleton<IMongoClient>(_ =>
            new MongoClient(mongoDbConfig.Get<MongoDbOptions>()?.ConnectionString)
        );

        builder.Services.AddSingleton<IDbClient, MongoDbClient>();

        var firebaseServiceAccountConfiguration = configuration.GetSection("Firebase:ServiceAccount").Get<Dictionary<string, object>>();
        var firebaseServiceAccountJson = JsonConvert.SerializeObject(firebaseServiceAccountConfiguration);

        FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.FromJson(firebaseServiceAccountJson),
            ProjectId = firebaseConfig.Get<FirebaseOptions>()?.ProjectId,
        });

        builder.Services.AddSingleton<Functions>();
        builder.Services.AddSingleton<ICheckNotificationsService, CheckNotificationsService>();
        builder.Services.AddSingleton<INotificationProducer, FirebaseNotificationProducer>();

        JsonConvert.DefaultSettings = () =>
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new StringEnumConverter());
            return settings;
        };
    }
}
