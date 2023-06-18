using System.Threading.RateLimiting;
using ALamb_clock_API.Middleware;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Shared.Clients;
using Shared.Options;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile("appsettings.local.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var mongoDbConfig = configuration.GetSection("MongoDb");
var authConfig = configuration.GetSection("Authentication");

builder.Services.Configure<MongoDbOptions>(mongoDbConfig);
builder.Services.AddScoped<IMongoClient>(_ =>
    new MongoClient(mongoDbConfig.Get<MongoDbOptions>()?.ConnectionString)
);

builder.Services.Configure<AuthenticationOptions>(authConfig);

builder.Services.AddScoped<IDbClient, MongoDbClient>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ALamb clock API",
        Version = "v1"
    });
});

builder.Services.AddRateLimiter(options => {
    options.AddFixedWindowLimiter("Fixed", opt => {
        opt.Window = TimeSpan.FromSeconds(1);
        opt.PermitLimit = 100;
        opt.QueueLimit = 100;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "ALamb clock API v1");
    options.DocumentTitle = "Test";
});

app.UseReDoc(options =>
{
    options.DocumentTitle = "ALamb clock API documentation";
    options.SpecUrl = "/swagger/v1/swagger.json";
    options.RoutePrefix = string.Empty;
});

app.MapControllers();

app.UseMiddleware<ApiKeyAuthenticationMiddleware>();
app.UseRateLimiter();

app.Run();
