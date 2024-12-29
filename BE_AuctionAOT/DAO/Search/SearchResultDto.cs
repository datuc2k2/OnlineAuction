namespace BE_AuctionAOT.DAO.Search
{
    public class SearchResultDto
    {
        public string? Type { get; set; } // "Auction", "User", "Post"
        public long Id { get; set; }
        public string? Name { get; set; } // ProductName, Username, Title
        public string? Description { get; set; } // Description (Auction), Content (Post), null (User)
        public string? Url { get; set; } // URL để điều hướng
    }
}
