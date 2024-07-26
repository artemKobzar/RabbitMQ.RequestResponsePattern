using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
using System.Threading.Channels;
using RabbitMQ.Client.Events;

namespace Client.Services
{
    public class RequestSender : IRequestSender
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private TaskCompletionSource<string> _responseCompletionSource;

        public RequestSender()
        {
            var factory = new ConnectionFactory() { HostName = "localhost"};
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            //var _responseQueue = _channel.QueueDeclare(queue: "", exclusive: true);
        }
        public Task<string> GetResponse()
        {
            var _responseQueue = _channel.QueueDeclare(queue: "", exclusive: true);
            _channel.QueueDeclare(queue: "request-queue", exclusive: false);
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (model, eventArgs) =>
            {
                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                Console.WriteLine($"Response recieved: {message}");
                _responseCompletionSource.TrySetResult(message);
            };
            _responseCompletionSource = new TaskCompletionSource<string>();
            _channel.BasicConsume(queue: _responseQueue.QueueName, autoAck: true, consumer: consumer);

            return _responseCompletionSource.Task;
        }

        public string SendRequest(string message)
        {
            var _responseQueue = _channel.QueueDeclare(queue: "", exclusive: true);
            //var jsonString = JsonSerializer.Serialize(request);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.ReplyTo = _responseQueue.QueueName;
            properties.CorrelationId = Guid.NewGuid().ToString();

            _channel.BasicPublish("", "request-queue", properties, body);

            Console.WriteLine("Client has been started");
            Console.WriteLine($"Sending request: {properties.CorrelationId}");
            Console.ReadKey();

            return (properties.CorrelationId);
        }

        //public async Task<string> SendMessage(string message, string correlationId)
        //{

        //}
    }
}

//return message;
