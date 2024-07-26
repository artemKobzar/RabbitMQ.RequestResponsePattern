using Client.Model;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections;
using System.Text;
using System.Text.Json;

namespace Client.Services
{
    public class RequestProducer : IRequestProducer
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _replyQueueName;
        private readonly EventingBasicConsumer _consumer;
        private readonly IBasicProperties _properties;
        private readonly Dictionary<string, TaskCompletionSource<string>> _callbackMapper = new();

        public RequestProducer()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _replyQueueName = _channel.QueueDeclare().QueueName;
            _consumer = new EventingBasicConsumer(_channel);
            _consumer.Received += (model, ea) =>
            {
                if (_callbackMapper.TryGetValue(ea.BasicProperties.CorrelationId, out var tcs))
                {
                    var body = ea.Body.ToArray();
                    var response = Encoding.UTF8.GetString(body);
                    tcs.SetResult(response);
                }
            };
            _channel.BasicConsume(consumer: _consumer, queue: _replyQueueName, autoAck: true);

            _properties = _channel.CreateBasicProperties();
            _properties.ReplyTo = _replyQueueName;
        }

        public Task<string> SendRequest(string message)
        {
            var correlationId = Guid.NewGuid().ToString();
            _properties.CorrelationId = correlationId;

            var messageBytes = Encoding.UTF8.GetBytes(message);
            var tcs = new TaskCompletionSource<string>();

            _callbackMapper[correlationId] = tcs;

            _channel.BasicPublish(exchange: "", routingKey: "request-queue", basicProperties: _properties, body: messageBytes);

            return tcs.Task;
        }
    }
}
