using System;
using System.Collections.Generic;

namespace BE_AuctionAOT.Models
{
    public partial class Like
    {
        public int Id { get; set; }
        public int? PostId { get; set; }
        public int? CommentId { get; set; }
        public long? AuctionReviewId { get; set; }
        public long UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
