namespace BE_AuctionAOT.Common.Base.Entity
{
    public class SendMailDTO
    {
        public string FromEmail { get; set; }
        public string Password { get; set; }
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
