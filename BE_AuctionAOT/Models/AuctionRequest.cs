using System;
using System.Collections.Generic;

namespace BE_AuctionAOT.Models
{
    public partial class AuctionRequest
    {
        public long RequestId { get; set; }
        public long AuctionId { get; set; }
        public long UserId { get; set; }
        public bool Type { get; set; }
        public string? RequestDetails { get; set; }
        public bool? IsApproved { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public long? ApprovedBy { get; set; }

        public virtual Auction Auction { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
