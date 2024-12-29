using System;
using System.Collections.Generic;

namespace BE_AuctionOT_Cronjob.Modelss
{
    public partial class PaymentHistory
    {
        public long PaymentId { get; set; }
        public long UserId { get; set; }
        public long AuctionId { get; set; }
        public decimal PaymentAmount { get; set; }
        public string Currency { get; set; } = null!;
        public DateTime? PaymentTime { get; set; }
        public string? Description { get; set; }
        public string AccountNumber { get; set; } = null!;
        public string PaymentCode { get; set; } = null!;

        public virtual User User { get; set; } = null!;
    }
}
