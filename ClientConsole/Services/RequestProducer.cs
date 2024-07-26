using ClientConsole.Model;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace ClientConsole.Services
{
    public class RequestProducer
    {
        public void  SendRequest(Order request)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "user",
                Password = "password",
                VirtualHost = "/",
                Port = AmqpTcpEndpoint.UseDefaultPort
            };
            IConnection connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            var responseQueue = channel.QueueDeclare(queue: "", exclusive: true);
            channel.QueueDeclare(queue: "request-queue", exclusive:  false);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, eventArgs) =>
            {
                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Response recieved: {message}");
            };

            channel.BasicConsume(queue: responseQueue.QueueName, autoAck: true, consumer: consumer);

            //Publish request
            var jsonString = JsonSerializer.Serialize(request);
            var body = Encoding.UTF8.GetBytes(jsonString);

            var properties = channel.CreateBasicProperties();
            properties.ReplyTo = responseQueue.QueueName;
            properties.CorrelationId = Guid.NewGuid().ToString();

            channel.BasicPublish("", "request-queue", properties, body);

            Console.WriteLine($"Sending request: {properties.CorrelationId}");

        }
    }
}
