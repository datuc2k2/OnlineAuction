using System;
using System.Collections.Generic;

namespace BE_AuctionAOT.Models
{
    public partial class Auction
    {
        public Auction()
        {
            AuctionBids = new HashSet<AuctionBid>();
            AuctionImages = new HashSet<AuctionImage>();
            AuctionInvitations = new HashSet<AuctionInvitation>();
            AuctionPosts = new HashSet<AuctionPost>();
            AuctionRequests = new HashSet<AuctionRequest>();
            AuctionReviews = new HashSet<AuctionReview>();
            AutoBids = new HashSet<AutoBid>();
            Chats = new HashSet<Chat>();
            Disputes = new HashSet<Dispute>();
        }

        public long AuctionId { get; set; }
        public long UserId { get; set; }
        public string ProductName { get; set; } = null!;
        public long CategoryId { get; set; }
        public decimal StartingPrice { get; set; }
        public string Currency { get; set; } = null!;
        public decimal StepPrice { get; set; }
        public long Mode { get; set; }
        public string Description { get; set; } = null!;
        public bool? IsPrivate { get; set; }
        public decimal? DepositAmount { get; set; }
        public DateTime? DepositDeadline { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public long? CreatedBy { get; set; }
        public long? UpdatedBy { get; set; }
        public long Status { get; set; }
        public long PaymentStatus { get; set; }

        public virtual Category Category { get; set; } = null!;
        public virtual Category ModeNavigation { get; set; } = null!;
        public virtual Category PaymentStatusNavigation { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual ICollection<AuctionBid> AuctionBids { get; set; }
        public virtual ICollection<AuctionImage> AuctionImages { get; set; }
        public virtual ICollection<AuctionInvitation> AuctionInvitations { get; set; }
        public virtual ICollection<AuctionPost> AuctionPosts { get; set; }
        public virtual ICollection<AuctionRequest> AuctionRequests { get; set; }
        public virtual ICollection<AuctionReview> AuctionReviews { get; set; }
        public virtual ICollection<AutoBid> AutoBids { get; set; }
        public virtual ICollection<Chat> Chats { get; set; }
        public virtual ICollection<Dispute> Disputes { get; set; }
    }
}
