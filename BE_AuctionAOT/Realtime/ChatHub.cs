using Microsoft.AspNetCore.SignalR;

namespace BE_AuctionAOT.Realtime
{
    public class ChatHub : Hub
    {
        public static Dictionary<string, string> userConnections = new Dictionary<string, string>();

        public async Task NotifyMessageToUser(int UserId)
        {
            if (userConnections.ContainsKey(UserId + ""))
            {
                string connectionId = userConnections[UserId+""];
                await Clients.Client(connectionId).SendAsync("ReceiveNotifyNewMessage", "You have a new message, let fetch api.");
            }
        }

        public override async Task OnConnectedAsync()
        {
            string userId = Context.GetHttpContext().Request.Query["userId"];
            string connectionId = Context.ConnectionId;

            // Add user to dictionary
            if (!userConnections.ContainsKey(userId))
            {
                userConnections.Add(userId, connectionId);
            }

            var activeUserIds = userConnections.Keys.ToList();
            await Clients.All.SendAsync("UserActiveIds", activeUserIds);

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

            var activeUserIds = userConnections.Keys.ToList();
            await Clients.All.SendAsync("UserActiveIds", activeUserIds);

            await base.OnDisconnectedAsync(exception);
        }
    }
}
