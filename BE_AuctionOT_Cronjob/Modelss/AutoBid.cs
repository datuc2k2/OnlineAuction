using System;
using System.Collections.Generic;

namespace BE_AuctionOT_Cronjob.Modelss
{
    public partial class AutoBid
    {
        public long AutoBidId { get; set; }
        public long AuctionId { get; set; }
        public long UserId { get; set; }
        public decimal MaxBidAmount { get; set; }
        public decimal? CurrentBidAmount { get; set; }
        public string Currency { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }

        public virtual Auction Auction { get; set; } = null!;
    }
}
