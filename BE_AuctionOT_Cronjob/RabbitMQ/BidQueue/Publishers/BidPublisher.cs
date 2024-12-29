using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace BE_AuctionOT_Cronjob.RabbitMQ.BidQueue.Publishers
{
    public class BidPublisher : IBidPublisher
    {
        private readonly IConnection _connection;

        public BidPublisher()
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                Port = 5672
            };
            _connection = factory.CreateConnection();
        }

        public void CreateANewQueue(string queueName)
        {
            try
            {
                using (var channel = _connection.CreateModel())
                {
                    channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating queue: {ex.Message}");
            }
        }

        public void PublishBid(Bid bid)
        {
            using (var channel = _connection.CreateModel())
            {
                string queueName = $"bidsQueue_{bid.AuctionId}";

                // Declare the queue for the specific auction
                channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                var message = JsonSerializer.Serialize(bid);
                var body = Encoding.UTF8.GetBytes(message);

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
            }
        }
    }

    public class Bid
    {
        public int AuctionId { get; set; }
        public int UserId { get; set; }
        public decimal BidAmount { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
