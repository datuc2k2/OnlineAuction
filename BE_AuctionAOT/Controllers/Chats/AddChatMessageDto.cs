namespace BE_AuctionAOT.Controllers.Chats
{
    public class AddChatMessageDto
    {
        public long ChatId { get; set; }
        public long SenderId { get; set; }
        public string? ContentText { get; set; } = null!;
        public IFormFile? ContentImage { get; set; } = null!;
    }
}
