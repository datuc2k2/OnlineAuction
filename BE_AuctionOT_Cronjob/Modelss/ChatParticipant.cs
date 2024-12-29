using System;
using System.Collections.Generic;

namespace BE_AuctionOT_Cronjob.Modelss
{
    public partial class ChatParticipant
    {
        public long ChatId { get; set; }
        public long UserId { get; set; }
        public DateTime? JoinedAt { get; set; }

        public virtual Chat Chat { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
