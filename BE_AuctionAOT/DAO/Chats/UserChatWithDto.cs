namespace BE_AuctionAOT.DAO.Chats
{
    public class UserChatWithDto
    {
        public int UserIdWith { get; set; }
        public string UserNameWith { get; set; }
        public string UserAvatarWidth { get; set; }
        public int ChatId { get; set; }
        public bool IsAdmin { get; set; }
        public List<ChatMessageDto> ChatMessages { get; set; }
    }

    public class ChatMessageDto
    {
        public int MessageID { get; set; }
        public int ChatID { get; set; }
        public int SenderID { get; set; }
        public string SenderName { get; set; }
        public string SenderAvatar { get; set; }
        public string Content { get; set; }
        public DateTime? SendAt { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? IsRead { get; set; }
    }

}
