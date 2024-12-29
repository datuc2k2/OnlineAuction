using System;
using System.Collections.Generic;

namespace BE_AuctionOT_Cronjob.Modelss
{
    public partial class UserProfile
    {
        public long UserId { get; set; }
        public string? FullName { get; set; }
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Avatar { get; set; }
        public string? Cccd { get; set; }
        public string? FrontIdCard { get; set; }
        public string? BackIdCard { get; set; }
        public DateTime? Dob { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public long? UpdatedBy { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
