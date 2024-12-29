using System;
using System.Collections.Generic;

namespace BE_AuctionAOT.Models
{
    public partial class EvidenceFile
    {
        public long EvidenceId { get; set; }
        public long DisputeId { get; set; }
        public long UploadedBy { get; set; }
        public string FileUrl { get; set; } = null!;
        public string? FileType { get; set; }
        public DateTime? UploadedAt { get; set; }

        public virtual Dispute Dispute { get; set; } = null!;
        public virtual User UploadedByNavigation { get; set; } = null!;
    }
}
