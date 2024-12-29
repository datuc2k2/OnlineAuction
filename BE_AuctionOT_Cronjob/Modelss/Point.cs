using System;
using System.Collections.Generic;

namespace BE_AuctionOT_Cronjob.Modelss
{
    public partial class Point
    {
        public long PointId { get; set; }
        public long UserId { get; set; }
        public decimal PointsAmount { get; set; }
        public string Currency { get; set; } = null!;
        public DateTime? LastUpdated { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
