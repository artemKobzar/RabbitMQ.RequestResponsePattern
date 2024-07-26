using ClientConsole.Model;
using ClientConsole.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using System.Text;


namespace ClientConsole
{
    public class Program
    {
        static void Main(string[] args)
        {
            var order = new Order { Id = Guid.NewGuid(), Name = "T-Shirt", Price = 10.00m, Created = new DateOnly(2024, 4, 18) };
            List<Order> Orders = new()
            {
                new Order {Id = Guid.NewGuid(), Name = "T-Shirt", Price = 10.00m, Created = new DateOnly(2024, 4, 18)},
                new Order {Id = Guid.NewGuid(), Name = "Shoes", Price = 26.00m, Created = new DateOnly(2024, 4, 18)},
                new Order {Id = Guid.NewGuid(), Name = "Pants", Price = 15.00m, Created = new DateOnly(2024, 4, 18)}
            };
            var factory = new ConnectionFactory()
            {
                HostName = "localhost"};

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            var responseQueue = channel.QueueDeclare(queue: "", exclusive: true);
            channel.QueueDeclare(queue: "response-queue", exclusive: false);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, eventArgs) =>
            {
                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Order order = JsonSerializer.Deserialize<Order>(message);
                //List<Order> Orders = JsonSerializer.Deserialize<List<Order>>(message);
                //if (Orders.Count > 0)
                //{
                //    foreach(var order in Orders)
                //    {
                //        Console.WriteLine($"Response recieved: Your orderId:'{order.Id}'  Name:'{order.Name}' with price {order.Price}$ was confirmed");
                //    }
                //}
                //else
                //    Console.WriteLine("You don't have any orders");
                Console.WriteLine($"Response recieved: Your orderId:'{order.Id}'  Name:'{order.Name}' with price {order.Price}$ was confirmed");
            };

            channel.BasicConsume(queue: responseQueue.QueueName, autoAck: true, consumer: consumer);

            //Publish request
            var jsonString = JsonSerializer.Serialize(order);
            var body = Encoding.UTF8.GetBytes(jsonString);

            var properties = channel.CreateBasicProperties();
            properties.ReplyTo = responseQueue.QueueName;
            properties.CorrelationId = Guid.NewGuid().ToString();

            channel.BasicPublish("", "request-queue", properties, body);

            Console.WriteLine("Client has been started");
            Console.WriteLine($"Sending request: {properties.CorrelationId}");

            Console.ReadKey();
        }
    }
}
//for (var i = 0; i < 8; i++)
//{
//    Thread.Sleep(2000);
//    Console.WriteLine("Doing another job...");
//}
