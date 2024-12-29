namespace BE_AuctionAOT.Controllers.AuctionForCustomer.JoinTheAuction
{
    public class ChatMessageDto
    {
        public long MessageId { get; set; }
        public long ChatId { get; set; }
        public long SenderId { get; set; }
        public string SenderName { get; set; }
        public string Content { get; set; } = null!;
        public DateTime? SentAt { get; set; }
        public bool? IsDeleted { get; set; }
        public int AuctionId { get; set; }
        public string Avatar { get; set; }
    }
}
