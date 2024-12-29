namespace BE_AuctionAOT.DAO.AuctionReviewDao
{
    public class AuctionReviewDto
    {
        public long ReviewId { get; set; }
        public long? AuctionId { get; set; }
        public long UserId { get; set; }
        public byte? Rating { get; set; }
        public string? Content { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int LikesCount { get; set; } = 0;
        public bool IsLiked { get; set; } = false;
        public int SubCommentsCount { get; set; } = 0;
        public AuctionReviewUserDto? Owner { get; set; }
    }
}
