using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

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

app.Run();