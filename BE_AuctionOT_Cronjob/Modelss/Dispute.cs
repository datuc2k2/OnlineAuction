using System;
using System.Collections.Generic;

namespace BE_AuctionOT_Cronjob.Modelss
{
    public partial class Dispute
    {
        public Dispute()
        {
            EvidenceFiles = new HashSet<EvidenceFile>();
        }

        public long DisputeId { get; set; }
        public long AuctionId { get; set; }
        public long WinnerId { get; set; }
        public long CreatorId { get; set; }
        public string? DisputeReason { get; set; }
        public bool? WinnerConfirmed { get; set; }
        public bool? CreatorConfirmed { get; set; }
        public string? WinnerEvidence { get; set; }
        public string? CreatorEvidence { get; set; }
        public string? AdminDecision { get; set; }
        public long DisputeStatusId { get; set; }
        public long? ResolvedBy { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public long? CreatedBy { get; set; }

        public virtual Auction Auction { get; set; } = null!;
        public virtual User Creator { get; set; } = null!;
        public virtual Category DisputeStatus { get; set; } = null!;
        public virtual User Winner { get; set; } = null!;
        public virtual ICollection<EvidenceFile> EvidenceFiles { get; set; }
    }
}
