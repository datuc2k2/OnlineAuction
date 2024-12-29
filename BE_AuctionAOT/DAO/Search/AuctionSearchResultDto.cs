namespace BE_AuctionAOT.DAO.Search
{
    public class AuctionSearchResultDto
    {
        public long AuctionId { get; set; }
        public string? ProductName { get; set; }
        public decimal? StartingPrice { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Description { get; set; }
        public string? CategoryName { get; set; }
        public long? Status { get; set; }
        public AuctionUserDto? User { get; set; }
        public List<AuctionImagetDto>? AuctionImages { get; set; }
    }

    public class AuctionUserDto
    {
        public long UserId { get; set;}
        public string? Username { get; set; }
    }
    public class AuctionImagetDto
    {
        public long ImageId { get; set; }
        public long AuctionId { get; set; }
        public string? ImageUrl { get; set; }
        public int? SortOrder { get; set; }
        public DateTime? CreatedAt { get; set; }
        public long? CreatedBy { get; set; }
    }
}
