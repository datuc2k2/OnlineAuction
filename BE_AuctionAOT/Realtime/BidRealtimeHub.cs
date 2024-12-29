using Microsoft.AspNetCore.SignalR;
using System;

namespace BE_AuctionAOT.Realtime
{
    public class BidRealtimeHub : Hub
    {
        public static Dictionary<string, string> userConnections = new Dictionary<string, string>();

        public async Task JoinAuctionGroup(string auctionId)
        {
            string groupName = $"auction_{auctionId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveAuctionGroup(string auctionId)
        {
            string groupName = $"auction_{auctionId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task SendNotificationToAuctionGroup(string auctionId, string message)
        {
            string groupName = $"auction_{auctionId}";
            await Clients.Group(groupName).SendAsync("ReceiveNotification", message);
        }

        public async Task SendMessage(string user, string message)
        {
            if (userConnections.ContainsKey(user))
            {
                string connectionToUser = userConnections[user];
                await Clients.Client(connectionToUser).SendAsync("ReceiveMessage", message);
            }
        }

        public override async Task OnConnectedAsync()
        {
            string userId = Context.GetHttpContext().Request.Query["userId"];
            if(userId != "0")
            {
                string connectionId = Context.ConnectionId;

                // Add user to dictionary
                if (!userConnections.ContainsKey(userId))
                {
                    userConnections.Add(userId, connectionId);
                }
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string userId = Context.GetHttpContext().Request.Query["userId"];

            // Remove user from dictionary
            if (userConnections.ContainsKey(userId))
            {
                userConnections.Remove(userId);
            }

            await base.OnDisconnectedAsync(exception);
        }

    }
}
