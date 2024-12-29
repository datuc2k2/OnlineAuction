namespace BE_AuctionAOT.DAO.Search
{
    public class PostSearchResultDto
    {
        public string? Content { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? total_like { get; set; }
        public int? total_comment { get; set; }
        public PostUserDto? Owner { get; set; }

    }
    public class PostUserDto
    {
        public long UserId { get; set; }
        public string? Name { get; set; }
        public string? Avatar { get; set; }
    }
}
