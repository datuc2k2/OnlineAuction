using System;
using System.Collections.Generic;

namespace BE_AuctionOT_Cronjob.Modelss
{
    public partial class AuctionImage
    {
        public long ImageId { get; set; }
        public long AuctionId { get; set; }
        public string ImageUrl { get; set; } = null!;
        public int? SortOrder { get; set; }
        public DateTime? CreatedAt { get; set; }
        public long? CreatedBy { get; set; }

        public virtual Auction Auction { get; set; } = null!;
    }
}
