using System;
using System.Collections.Generic;

namespace BE_AuctionOT_Cronjob.Modelss
{
    public partial class AuctionReview
    {
        public long ReviewId { get; set; }
        public long AuctionId { get; set; }
        public long UserId { get; set; }
        public byte Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual Auction Auction { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
