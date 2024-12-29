using System;
using System.Collections.Generic;

namespace BE_AuctionAOT.Models
{
    public partial class AuctionPost
    {
        public long AuctionPostId { get; set; }
        public int PostId { get; set; }
        public long AuctionId { get; set; }
        public DateTime? CreatedAt { get; set; }

        public virtual Auction Auction { get; set; } = null!;
        public virtual Post Post { get; set; } = null!;
    }
}
