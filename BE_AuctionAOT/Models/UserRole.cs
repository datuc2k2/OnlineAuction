using System;
using System.Collections.Generic;

namespace BE_AuctionAOT.Models
{
    public partial class UserRole
    {
        public long UserId { get; set; }
        public long RoleId { get; set; }
        public DateTime? AssignedAt { get; set; }
        public long? AssignedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public long? UpdatedBy { get; set; }

        public virtual Role Role { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
