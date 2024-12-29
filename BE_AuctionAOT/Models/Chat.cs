using System;
using System.Collections.Generic;

namespace BE_AuctionAOT.Models
{
    public partial class Chat
    {
        public Chat()
        {
            ChatMessages = new HashSet<ChatMessage>();
            ChatParticipants = new HashSet<ChatParticipant>();
        }

        public long ChatId { get; set; }
        public long? AuctionId { get; set; }
        public bool? IsGroupChat { get; set; }
        public DateTime? CreatedAt { get; set; }
        public long CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public long? UpdatedBy { get; set; }

        public virtual Auction? Auction { get; set; }
        public virtual ICollection<ChatMessage> ChatMessages { get; set; }
        public virtual ICollection<ChatParticipant> ChatParticipants { get; set; }
    }
}
