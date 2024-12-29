using System;
using System.Collections.Generic;

namespace BE_AuctionAOT.Models
{
    public partial class PointTransaction
    {
        public long TransactionId { get; set; }
        public long UserId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public DateTime? TransactionTime { get; set; }
        public string? Description { get; set; }
        public string? TransactionCode { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
