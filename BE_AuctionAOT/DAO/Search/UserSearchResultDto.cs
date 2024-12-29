namespace BE_AuctionAOT.DAO.Search
{
    public class UserSearchResultDto
    {
        public long UserId { get; set; }
        public string Username { get; set; }
        public string? Avatar { get; set; }
        public double? AverageRating { get; set; }
        public int? ReviewCount { get; set; }
    }
}
