namespace BE_AuctionOT_Cronjob.Common.Base.Entity
{
    public class BaseOutputDto
    {
        public int ResultCd { get; set; }
        public OutputMessages? Messages { get; set; }

        public Exception? Exception { get; set; }
    }

    public class OutputMessages
    {
        public List<OutputMessage>? Common { get; set; }
        public List<OutputMessage>? Validation { get; set; }
    }

    public class OutputMessage
    {
        public string? MessageCd { get; set; }
        public string[]? Parameters { get; set; }
        public string? Information { get; set; }
    }
}
