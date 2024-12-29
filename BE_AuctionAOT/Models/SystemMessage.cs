using System;
using System.Collections.Generic;

namespace BE_AuctionAOT.Models
{
    public partial class SystemMessage
    {
        public long MessageId { get; set; }
        public string Code { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string? LanguageCode { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
