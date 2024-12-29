using System;
using System.Collections.Generic;

namespace BE_AuctionAOT.Models
{
    public partial class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public int PostId { get; set; }
        public long UserId { get; set; }
        public int? ParentId { get; set; }
    }
}
