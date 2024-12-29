using System;
using System.Collections.Generic;

namespace BE_AuctionAOT.Models
{
    public partial class Post
    {
        public Post()
        {
            AuctionPosts = new HashSet<AuctionPost>();
            PostImages = new HashSet<PostImage>();
        }
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public PostStatus Status { get; set; } = PostStatus.Pending;
        public DateTime CreatedAt { get; set; }
        public long UserId { get; set; }
        public string Title { get; set; } = null!;
        public long? CategoryId { get; set; }
        public virtual Category? Category { get; set; }
        public virtual ICollection<AuctionPost> AuctionPosts { get; set; }
        public virtual ICollection<PostImage> PostImages { get; set; }
    }
    public enum PostStatus
    {
        Pending,
        Approved,
        Rejected
    }
}
