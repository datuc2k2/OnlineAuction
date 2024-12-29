using System;
using System.Collections.Generic;

namespace BE_AuctionAOT.Models
{
    public partial class ChatMessage
    {
        public long MessageId { get; set; }
        public long ChatId { get; set; }
        public long SenderId { get; set; }
        public string Content { get; set; } = null!;
        public DateTime? SentAt { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? IsRead { get; set; }

        public virtual Chat Chat { get; set; } = null!;
        public virtual User Sender { get; set; } = null!;
    }
}
