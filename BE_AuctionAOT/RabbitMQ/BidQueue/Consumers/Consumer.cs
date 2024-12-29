using BE_AuctionAOT.DAO.AuctionForCustomer.JoinTheAuction;
using BE_AuctionAOT.Models;
using BE_AuctionAOT.RabbitMQ.BidQueue.Publishers;
using BE_AuctionAOT.Realtime;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;

namespace BE_AuctionAOT.RabbitMQ.BidQueue.Consumers
{
    public class Consumer : IConsumer
    {
        private readonly IConnection _connection;
        private readonly JoinTheAuctionDao _joinTheAuctionDao;
        private readonly DB_AuctionAOTContext _context;
        private readonly IHubContext<BidRealtimeHub> _hubContext;
        public Consumer(JoinTheAuctionDao joinTheAuctionDao, DB_AuctionAOTContext context, IHubContext<BidRealtimeHub> hubContext)
        {
            var factory = new ConnectionFactory() {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                Port = 5672
            };
            _connection = factory.CreateConnection();
            _joinTheAuctionDao = joinTheAuctionDao;
            _context = context;
            _hubContext = hubContext;
        }

        public async Task StartListening(int auctionId)
        {
            var channel = _connection.CreateModel();
            string queueName = $"bidsQueue_{auctionId}";

            try
            {
                // Declare the specific queue for the auction
                channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += async (model, ea) =>
                {
                    bool messageAcknowledged = false;

                    try
                    {
                        // Deserialize the message
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        var bid = JsonSerializer.Deserialize<Bid>(message);

                        if (bid != null)
                        {
                            // Process the bid
                            string result = HandleBid(bid);

                            // Notify the client about bid handling result
                            if (BidRealtimeHub.userConnections.TryGetValue(bid.UserId.ToString(), out var connectionId))
                            {
                                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveMessage", result);
                            }

                            // Notify the group if the bid was successful
                            if (result == "Bid successfully")
                            {
                                string groupName = $"auction_{bid.AuctionId}";
                                await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", "New bid, let re-fetch api");
                            }
                        }

                        // Acknowledge the message
                        if (channel.IsOpen)
                        {
                            channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                            messageAcknowledged = true;
                        }
                        else
                        {
                            Console.Error.WriteLine("Channel is closed; cannot acknowledge the message.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error processing message: {ex.Message}");

                        // Retry mechanism for Nack if message wasn't already acknowledged
                        if (!messageAcknowledged && channel.IsOpen)
                        {
                            int retryCount = 3;
                            while (retryCount > 0)
                            {
                                try
                                {
                                    channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                                    Console.WriteLine("Message negatively acknowledged and requeued.");
                                    break;
                                }
                                catch (Exception nackEx)
                                {
                                    retryCount--;
                                    Console.Error.WriteLine($"Error during Nack. Retries left: {retryCount}. Error: {nackEx.Message}");
                                }
                            }
                        }
                        else
                        {
                            Console.Error.WriteLine("Could not Nack the message; channel might be closed.");
                        }
                    }
                };

                channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error initializing listener for auction {auctionId}: {ex.Message}");
            }
        }



        public string HandleBid(Bid bid)
        {
            Console.WriteLine($"Processing bid from User {bid.UserId} for Auction {bid.AuctionId}: ${bid.BidAmount}");

            var listAuctionBidByAuctionId = _joinTheAuctionDao.GetAllAuctionBidByAuctionId(bid.AuctionId);

            var maxAmountBid = 0;
            if (listAuctionBidByAuctionId != null && listAuctionBidByAuctionId.Any())
            {
                foreach (var auctionBid in listAuctionBidByAuctionId)
                {
                    if (auctionBid.BidAmount > maxAmountBid)
                    {
                        maxAmountBid = (int)Math.Ceiling(auctionBid.BidAmount);
                    }
                }
            }

            if (bid.BidAmount <= maxAmountBid)
            {
                return "Cannot bid less than current amount";
            }

            AuctionBid auctionBidCreate = new AuctionBid()
            {
                AuctionId = bid.AuctionId,
                UserId = bid.UserId,
                BidAmount = bid.BidAmount,
                Currency = "VND",
                BidTime = bid.Timestamp,
            };
            if(bid.UserId <= 0)
            {
                return "Bid failed";
            } 
            else
            {
                _context.AuctionBids.Add(auctionBidCreate);
                _context.SaveChanges();
            }

            if (auctionBidCreate.BidId != null)
            {
                return "Bid successfully";
            }
            return "Bid failed";
        }

    }
}