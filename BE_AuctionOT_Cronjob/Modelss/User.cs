using System;
using System.Collections.Generic;

namespace BE_AuctionOT_Cronjob.Modelss
{
    public partial class User
    {
        public User()
        {
            AuctionBids = new HashSet<AuctionBid>();
            AuctionInvitations = new HashSet<AuctionInvitation>();
            AuctionRequests = new HashSet<AuctionRequest>();
            AuctionReviews = new HashSet<AuctionReview>();
            Auctions = new HashSet<Auction>();
            AuditLogs = new HashSet<AuditLog>();
            ChatMessages = new HashSet<ChatMessage>();
            ChatParticipants = new HashSet<ChatParticipant>();
            Deposits = new HashSet<Deposit>();
            DisputeCreators = new HashSet<Dispute>();
            DisputeWinners = new HashSet<Dispute>();
            EvidenceFiles = new HashSet<EvidenceFile>();
            Notifications = new HashSet<Notification>();
            PasswordResetRequests = new HashSet<PasswordResetRequest>();
            PaymentHistories = new HashSet<PaymentHistory>();
            PointTransactions = new HashSet<PointTransaction>();
            Points = new HashSet<Point>();
            UserRoles = new HashSet<UserRole>();
        }

        public long UserId { get; set; }
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool? IsActive { get; set; }
        public DateTime? LastLogin { get; set; }
        public string? VerifyToken { get; set; }
        public DateTime? VerifyTokenExpiresAt { get; set; }
        public bool? EmailVerified { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public long? CreatedBy { get; set; }
        public long? UpdatedBy { get; set; }

        public virtual UserProfile? UserProfile { get; set; }
        public virtual ICollection<AuctionBid> AuctionBids { get; set; }
        public virtual ICollection<AuctionInvitation> AuctionInvitations { get; set; }
        public virtual ICollection<AuctionRequest> AuctionRequests { get; set; }
        public virtual ICollection<AuctionReview> AuctionReviews { get; set; }
        public virtual ICollection<Auction> Auctions { get; set; }
        public virtual ICollection<AuditLog> AuditLogs { get; set; }
        public virtual ICollection<ChatMessage> ChatMessages { get; set; }
        public virtual ICollection<ChatParticipant> ChatParticipants { get; set; }
        public virtual ICollection<Deposit> Deposits { get; set; }
        public virtual ICollection<Dispute> DisputeCreators { get; set; }
        public virtual ICollection<Dispute> DisputeWinners { get; set; }
        public virtual ICollection<EvidenceFile> EvidenceFiles { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<PasswordResetRequest> PasswordResetRequests { get; set; }
        public virtual ICollection<PaymentHistory> PaymentHistories { get; set; }
        public virtual ICollection<PointTransaction> PointTransactions { get; set; }
        public virtual ICollection<Point> Points { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
