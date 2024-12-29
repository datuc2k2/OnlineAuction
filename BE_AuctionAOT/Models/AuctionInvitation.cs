using System;
using System.Collections.Generic;

namespace BE_AuctionAOT.Models
{
    public partial class AuctionInvitation
    {
        public long InvitationId { get; set; }
        public long AuctionId { get; set; }
        public long InvitedUserId { get; set; }
        public bool? IsAccepted { get; set; }
        public DateTime? InvitedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }

        public virtual Auction Auction { get; set; } = null!;
        public virtual User InvitedUser { get; set; } = null!;
    }
}
