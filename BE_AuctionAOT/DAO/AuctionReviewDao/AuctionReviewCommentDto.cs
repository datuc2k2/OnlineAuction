using BE_AuctionAOT.Models;

namespace BE_AuctionAOT.DAO.AuctionReviewDao
{
    public class AuctionReviewCommentDto
    {
        public long ReviewId { get; set; }
        public long? AuctionId { get; set; }
        public long? ParentId { get; set; }
        public long UserId { get; set; }
        public byte? Rating { get; set; }
        public string? Content { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int LikesCount { get; set; } = 0;
        public bool IsLiked { get; set; } = false;
        public AuctionReviewUserDto? Owner { get; set; }

        public List<AuctionReviewCommentDto> SubComments { get; set; } = new List<AuctionReviewCommentDto>();
        public List<AuctionReviewImageDto> Images { get; set; } = new List<AuctionReviewImageDto>();
        public bool IsReviewAboutAuction { get; set; }
        public bool IsCommentAboutReview { get; set; }
        public bool IsReplyComment { get; set; }
    }
    public class AuctionReviewImageDto
    {
        public long ImageId { get; set; }
        public long ReviewId { get; set; }
        public string? ImageUrl { get; set; }
    }
    public class AuctionReviewUserDto 
    {
        public long UserId { get; set; }
        public string? Avatar { get; set; }
        public string? Username { get; set; }
    }
}
