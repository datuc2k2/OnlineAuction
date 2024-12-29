using System;
using System.Collections.Generic;

namespace BE_AuctionAOT.Models
{
    public partial class AuditLog
    {
        public long LogId { get; set; }
        public long UserId { get; set; }
        public string Action { get; set; } = null!;
        public DateTime? Timestamp { get; set; }
        public string? Details { get; set; }
        public long? PerformedBy { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
