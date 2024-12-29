using System;
using System.Collections.Generic;

namespace BE_AuctionOT_Cronjob.Modelss
{
    public partial class Post
    {
        public Post()
        {
            AuctionPosts = new HashSet<AuctionPost>();
        }

        public int Id { get; set; }
        public string Content { get; set; } = null!;
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public long UserId { get; set; }

        public virtual ICollection<AuctionPost> AuctionPosts { get; set; }
    }
}
