using System;
using System.Collections.Generic;

namespace BE_AuctionAOT.Models
{
    public partial class Category
    {
        public Category()
        {
            AuctionCategories = new HashSet<Auction>();
            AuctionModeNavigations = new HashSet<Auction>();
            AuctionPaymentStatusNavigations = new HashSet<Auction>();
            Disputes = new HashSet<Dispute>();
            Posts = new HashSet<Post>();
        }

        public long CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public int Type { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public long? CreatedBy { get; set; }
        public long? UpdatedBy { get; set; }
        public decimal? Value { get; set; }

        public virtual ICollection<Auction> AuctionCategories { get; set; }
        public virtual ICollection<Auction> AuctionModeNavigations { get; set; }
        public virtual ICollection<Auction> AuctionPaymentStatusNavigations { get; set; }
        public virtual ICollection<Dispute> Disputes { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
    }
}
