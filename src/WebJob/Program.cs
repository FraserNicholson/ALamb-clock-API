using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebJob
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new HostBuilder();
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
        }
    }   
}