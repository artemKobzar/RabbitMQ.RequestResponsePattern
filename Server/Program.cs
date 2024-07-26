using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Web;

namespace Server
{
    public class Program
    {
        static void Main(string[] args)
        {
            List<Order> orders = new List<Order>();
            var factory = new ConnectionFactory()
            { HostName = "localhost"};
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            //channel.QueueDeclare(queue: "request-queue", exclusive: false, autoDelete: false, arguments: null);
            //channel.QueueDeclare(queue: "response-queue", exclusive: false);
            channel.QueueDeclare(queue: "request-queue", exclusive: false);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, eventArgs) =>
            {
                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var replyProperties = channel.CreateBasicProperties();
                replyProperties.CorrelationId = eventArgs.BasicProperties.CorrelationId;
                //var responseBytes = Encoding.UTF8.GetBytes(message);
                Console.WriteLine($"Recived request: {eventArgs.BasicProperties.CorrelationId}");
                Order order = JsonSerializer.Deserialize<Order>(message);
                //List<Order> orders = JsonSerializer.Deserialize<List<Order>>(message);
                //orders.Add(order);
                if (order != null)
                {
                    Console.WriteLine($"Response for request:");
                    Console.WriteLine($"order Id: {order.Id}, order Name: {order.Name} was added");
                }
                //if (orders.Any<Order>())
                //{
                //    Console.WriteLine($"Response for request:");
                //    foreach (var item in orders)
                //    {
                //        //Thread.Sleep(3000);
                //        Console.WriteLine($"order Id: {item.Id}, order Name: {item.Name} was added");
                //    }
                //}
                else
                    Console.WriteLine("List of orders is empty");

                channel.BasicPublish("", eventArgs.BasicProperties.ReplyTo, null, body);
                //channel.BasicPublish(exchange: "", routingKey: eventArgs.BasicProperties.ReplyTo, basicProperties: replyProperties, body: responseBytes);
            };
            channel.BasicConsume(queue: "request-queue", autoAck: true, consumer: consumer);
            Console.WriteLine("Server is running...");
            Console.ReadLine();
        }
    }
}
