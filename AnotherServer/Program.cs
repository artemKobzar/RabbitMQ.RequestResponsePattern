using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace AnotherServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            List<Order> orders = new List<Order>();
            var factory = new ConnectionFactory()
            {
                HostName = "localhost"
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "request-queue", exclusive: false);
            channel.BasicQos(prefetchSize:0, prefetchCount:1,global: false);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, eventArgs) =>
            {
                Task.Delay(4000).Wait();
                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Order order = JsonSerializer.Deserialize<Order>(message);
                //orders.Add(order);
                Console.WriteLine($"Recived request: {eventArgs.BasicProperties.CorrelationId}");

                channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
                channel.BasicPublish("", eventArgs.BasicProperties.ReplyTo, null, body);
            };
            channel.BasicConsume(queue: "request-queue", autoAck: false, consumer: consumer);

            Console.ReadKey();
        }
    }
}
