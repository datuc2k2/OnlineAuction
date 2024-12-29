namespace BE_AuctionAOT.Controllers.AuctionForCustomer.JoinTheAuction
{
    public class AuctionReviewInputDto
    {
        public long AuctionId { get; set; }
        public long UserId { get; set; }
        public byte Rating { get; set; }
        public string? Comment { get; set; }
    }
}
