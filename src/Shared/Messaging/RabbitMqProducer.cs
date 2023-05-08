using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Shared.Options;

namespace Shared.Messaging
{
    public interface IMessageProducer
    {
        void SendMessage<T> (T message);
    }

    public class RabbitMqProducer : IMessageProducer
    {
        private readonly RabbitMqOptions _rabbitMqOptions;

        public RabbitMqProducer(IOptions<RabbitMqOptions> rabbitMqOptions)
        {
            _rabbitMqOptions = rabbitMqOptions.Value;
        }

        public void SendMessage<T>(T message)
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri(_rabbitMqOptions.Uri)
            };
            
            var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            
            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);

            channel.BasicPublish(exchange: "", routingKey: _rabbitMqOptions.QueueName, body: body);
        }
    }
}