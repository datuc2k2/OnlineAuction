using System;
using System.Collections.Generic;

namespace BE_AuctionAOT.Models
{
    public partial class AuctionBid
    {
        public long BidId { get; set; }
        public long AuctionId { get; set; }
        public long UserId { get; set; }
        public decimal BidAmount { get; set; }
        public string Currency { get; set; } = null!;
        public DateTime? BidTime { get; set; }

        public virtual Auction Auction { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
