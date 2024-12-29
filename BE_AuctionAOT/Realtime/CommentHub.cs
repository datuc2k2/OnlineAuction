using Microsoft.AspNetCore.SignalR;

namespace BE_AuctionAOT.Realtime;

public class CommentHub : Hub
{
    /// <summary>
    /// Sends a message to all connected clients.
    /// </summary>
    /// <param name="message">The message to send.</param>
    public async Task SystemAppSendMessage(object message)
    {
        await Clients.All.SendAsync("SystemAppReceiveMessage", message);
    }


}