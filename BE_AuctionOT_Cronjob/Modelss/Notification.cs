using System;
using System.Collections.Generic;

namespace BE_AuctionOT_Cronjob.Modelss
{
    public partial class Notification
    {
        public long NotificationId { get; set; }
        public long? UserId { get; set; }
        public string Title { get; set; } = null!;
        public string? Content { get; set; }
        public int Type { get; set; }
        public bool? IsRead { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? SentAt { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public long? UpdatedBy { get; set; }

        public virtual User? User { get; set; }
    }
}
