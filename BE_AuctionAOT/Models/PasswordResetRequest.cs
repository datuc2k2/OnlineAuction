using System;
using System.Collections.Generic;

namespace BE_AuctionAOT.Models
{
    public partial class PasswordResetRequest
    {
        public long RequestId { get; set; }
        public long UserId { get; set; }
        public string Email { get; set; } = null!;
        public string ResetToken { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public bool? IsUsed { get; set; }
        public DateTime? CreatedAt { get; set; }
        public long? CreatedBy { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
