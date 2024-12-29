using Microsoft.AspNetCore.SignalR;

namespace BE_AuctionAOT.Realtime;

public class NotifyHub : Hub
{

    // New notice to all clients
    public async Task SystemAppSendNotice(object message)
    {
        await Clients.All.SendAsync("SystemAppReceiveNotice", message);
    }
}