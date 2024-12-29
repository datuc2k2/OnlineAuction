using System;
using System.Collections.Generic;

namespace BE_AuctionOT_Cronjob.Modelss
{
    public partial class Deposit
    {
        public long DepositId { get; set; }
        public long AuctionId { get; set; }
        public long UserId { get; set; }
        public decimal DepositAmount { get; set; }
        public string Currency { get; set; } = null!;
        public string? DepositStatus { get; set; }
        public DateTime? DepositDate { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
