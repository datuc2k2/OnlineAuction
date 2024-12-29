using BE_AuctionAOT.Models;

namespace BE_AuctionAOT.Controllers.Common.Notifications;

public class NotificationDto : Notification
{
    public List<long> UserIds { get; set; } = new();
}
