using System;
using System.Collections.Generic;

namespace BE_AuctionAOT.Models
{
    public partial class AuctionReview
    {
        public long ReviewId { get; set; }
        public long? AuctionId { get; set; } // Nếu có AuctionId, đó là review về món đồ đấu giá

        public long? ToUserId { get; set; }

        public long UserId { get; set; } // Người viết review hoặc comment
        public byte? Rating { get; set; } // Rating, chỉ có giá trị nếu đây là review cho món đồ đấu giá
        public string? Comment { get; set; }  // Comment, có thể về auction hoặc về review khác
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public long? ParentId { get; set; } // Nếu đây là comment về một review khác hoặc comment thì có ParentId
        public int LikesCount { get; set; } = 0;

        public virtual Auction? Auction { get; set; } = null!;
        public virtual User User { get; set; } = null!;

        public virtual User? ToUser { get; set; } = null!;

        public AuctionReview? Parent { get; set; }
        public virtual ICollection<AuctionReview> SubComments { get; set; } = new List<AuctionReview>();
        public virtual ICollection<AuctionReviewImage> Images { get; set; } = new List<AuctionReviewImage>();

        // Kiểm tra xem đây có phải là review về auction
        public bool IsReviewAboutAuction => AuctionId.HasValue;
        // Kiểm tra xem đây có phải là comment về review
        public bool IsCommentAboutReview => !AuctionId.HasValue;

        public bool IsReplyComment => !AuctionId.HasValue && Parent?.AuctionId.HasValue == false;

    }
}
