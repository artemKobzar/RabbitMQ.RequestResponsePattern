using AnotherClientConsole.Model;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace AnotherClientConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            List<Order> Orders = new()
            {
                new Order {Id = Guid.NewGuid(), Name = "T-Shirt", Price = 10.00m, Created = new DateOnly(2024, 4, 18)},
                new Order {Id = Guid.NewGuid(), Name = "Shoes", Price = 26.00m, Created = new DateOnly(2024, 4, 18)},
                new Order {Id = Guid.NewGuid(), Name = "Sport Shoes", Price = 19.00m, Created = new DateOnly(2024, 4, 18)},
                new Order {Id = Guid.NewGuid(), Name = "T-Shirt", Price = 6.00m, Created = new DateOnly(2024, 4, 18)},
                new Order {Id = Guid.NewGuid(), Name = "Coat", Price = 36.00m, Created = new DateOnly(2024, 4, 18)},
                new Order {Id = Guid.NewGuid(), Name = "Jeans", Price = 16.00m, Created = new DateOnly(2024, 4, 18)},
                new Order {Id = Guid.NewGuid(), Name = "Socks", Price = 3.00m, Created = new DateOnly(2024, 4, 18)},
                new Order {Id = Guid.NewGuid(), Name = "Shirt", Price = 12.00m, Created = new DateOnly(2024, 4, 18)},
                new Order {Id = Guid.NewGuid(), Name = "Pants", Price = 15.00m, Created = new DateOnly(2024, 4, 18)}
            };
            var factory = new ConnectionFactory()
            {
                HostName = "localhost"
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            var responseQueue = channel.QueueDeclare(queue: "", exclusive: true);
            channel.QueueDeclare(queue: "request-queue", exclusive: false);

            //Consumer
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, eventArgs) =>
            {
                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Order order = JsonSerializer.Deserialize<Order>(message);
                //List<Order> Orders = JsonSerializer.Deserialize<List<Order>>(message);
                Console.WriteLine($"Response for request recieved: Your orderId:'{order.Id}'  Name:'{order.Name}' with price {order.Price}$ was confirmed");

            };

            channel.BasicConsume(queue: responseQueue.QueueName, autoAck: true, consumer: consumer);

            //Publish request

            foreach(var order in Orders)
            {
                Task.Delay(3000).Wait();
                var jsonString = JsonSerializer.Serialize(order);
                var body = Encoding.UTF8.GetBytes(jsonString);

                var properties = channel.CreateBasicProperties();
                properties.ReplyTo = responseQueue.QueueName;
                properties.CorrelationId = Guid.NewGuid().ToString();

                channel.BasicPublish("", "request-queue", properties, body);

                Console.WriteLine($"Sending request: {properties.CorrelationId}");
            }
            Console.WriteLine("Another Client has been started");
            Console.ReadKey();
        }
    }
}
