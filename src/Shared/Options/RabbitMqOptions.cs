namespace Shared.Options
{
    public class RabbitMqOptions
    {
        public string Uri { get; set; } = default!;
        public string QueueName { get; set; } = default!;
    }
}