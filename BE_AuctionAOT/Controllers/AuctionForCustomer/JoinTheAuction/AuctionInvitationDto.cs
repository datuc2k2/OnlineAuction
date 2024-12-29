namespace BE_AuctionAOT.Controllers.AuctionForCustomer.JoinTheAuction
{
    public class AuctionInvitationDto
    {
        public long InvitationId { get; set; }
        public long AuctionId { get; set; }
        public long InvitedUserId { get; set; }
        public bool? IsAccepted { get; set; }
        public DateTime? InvitedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public long InviterId { get; set; }
        public string InviterAvatar { get; set; }
        public string InviterName { get; set; }
    }
}
