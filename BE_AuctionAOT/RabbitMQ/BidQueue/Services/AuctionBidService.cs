using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BE_AuctionAOT.DAO.AuctionForCustomer.JoinTheAuction;
using BE_AuctionAOT.Models;
using BE_AuctionAOT.RabbitMQ.BidQueue.Consumers;
using BE_AuctionAOT.Realtime;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace BE_AuctionAOT.RabbitMQ.BidQueue.Services
{
    public class AuctionBidService : IHostedService
    {
        private readonly List<IConsumer> _consumers = new();
        private readonly HttpClient _httpClient;
        private readonly IServiceProvider _serviceProvider;
        private const string RabbitMqHost = "localhost";
        private const string RabbitMqManagementApiUrl = "http://localhost:15672/api/queues";

        public AuctionBidService(HttpClient httpClient, IServiceProvider serviceProvider)
        {
            _httpClient = httpClient;
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Task.Run(async () =>
            {
                using (var scope = _serviceProvider.CreateScope()) // Create a new scope for each operation
                {
                    var joinTheAuctionDao = scope.ServiceProvider.GetRequiredService<JoinTheAuctionDao>();
                    var context = scope.ServiceProvider.GetRequiredService<DB_AuctionAOTContext>();
                    var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<BidRealtimeHub>>();

                    var existingQueueNames = new HashSet<string>();

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        // Fetch the current list of queues from RabbitMQ
                        List<string> queueNames = await GetQueueNamesFromRabbitMqAsync();

                        foreach (var queueName in queueNames)
                        {
                            if (!existingQueueNames.Contains(queueName))
                            {
                                var queueNameSplit = queueName.Split('_');

                                if (queueNameSplit.Length > 1 && int.TryParse(queueNameSplit[1], out int queueNumber))
                                {
                                    var consumer = new Consumer(joinTheAuctionDao, context, hubContext);

                                    // Add the consumer to the list
                                    existingQueueNames.Add(queueName);
                                    Task.Run(() => consumer.StartListening(queueNumber), cancellationToken);
                                }
                            }
                        }

                        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                    }
                }
            }, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Dispose resources if necessary

            return Task.CompletedTask;
        }

        private async Task<List<string>> GetQueueNamesFromRabbitMqAsync()
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, RabbitMqManagementApiUrl);

            // Add the RabbitMQ management API credentials (guest:guest)
            var byteArray = new System.Text.ASCIIEncoding().GetBytes("guest:guest");
            requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            var response = await _httpClient.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                var queues = JsonSerializer.Deserialize<List<RabbitMqQueue>>(responseData);

                // Extract queue names
                var queueNames = new List<string>();
                foreach (var queue in queues)
                {
                    queueNames.Add(queue.name);
                }
                return queueNames;
            }
            else
            {
                // Handle error
                return new List<string>();
            }
        }
    }
    public class RabbitMqQueue
    {
        public string name { get; set; }
    }
}