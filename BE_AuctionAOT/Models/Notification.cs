using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BE_AuctionAOT.Models
{
    public partial class Notification
    {
        public long NotificationId { get; set; }
        public long? UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int Type { get; set; }
        public bool? IsRead { get; set; }
        public bool? IsActive { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public long? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        public long? UpdatedBy { get; set; }

        [JsonIgnore]
        public virtual User? User { get; set; }
    }
}
