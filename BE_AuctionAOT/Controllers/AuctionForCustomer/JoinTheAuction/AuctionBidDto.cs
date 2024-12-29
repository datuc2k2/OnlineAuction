namespace BE_AuctionAOT.Controllers.AuctionForCustomer.JoinTheAuction
{
    public class AuctionBidDto
    {
        public long BidId { get; set; }
        public long AuctionId { get; set; }
        public long UserId { get; set; }
        public decimal BidAmount { get; set; }
        public string Currency { get; set; } = null!;
        public DateTime? BidTime { get; set; }
        public string? UserName { get; set; }
        public string ?UserAvatar { get; set; }
    }
}
