using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace BE_AuctionOT_Cronjob.Modelss
{
    public partial class DB_AuctionAOTContext : DbContext
    {
        public DB_AuctionAOTContext()
        {
        }

        public DB_AuctionAOTContext(DbContextOptions<DB_AuctionAOTContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Auction> Auctions { get; set; } = null!;
        public virtual DbSet<AuctionBid> AuctionBids { get; set; } = null!;
        public virtual DbSet<AuctionImage> AuctionImages { get; set; } = null!;
        public virtual DbSet<AuctionInvitation> AuctionInvitations { get; set; } = null!;
        public virtual DbSet<AuctionPost> AuctionPosts { get; set; } = null!;
        public virtual DbSet<AuctionRequest> AuctionRequests { get; set; } = null!;
        public virtual DbSet<AuctionReview> AuctionReviews { get; set; } = null!;
        public virtual DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public virtual DbSet<AutoBid> AutoBids { get; set; } = null!;
        public virtual DbSet<Category> Categories { get; set; } = null!;
        public virtual DbSet<Chat> Chats { get; set; } = null!;
        public virtual DbSet<ChatMessage> ChatMessages { get; set; } = null!;
        public virtual DbSet<ChatParticipant> ChatParticipants { get; set; } = null!;
        public virtual DbSet<Comment> Comments { get; set; } = null!;
        public virtual DbSet<Deposit> Deposits { get; set; } = null!;
        public virtual DbSet<Dispute> Disputes { get; set; } = null!;
        public virtual DbSet<EvidenceFile> EvidenceFiles { get; set; } = null!;
        public virtual DbSet<Like> Likes { get; set; } = null!;
        public virtual DbSet<Notification> Notifications { get; set; } = null!;
        public virtual DbSet<PasswordResetRequest> PasswordResetRequests { get; set; } = null!;
        public virtual DbSet<PaymentHistory> PaymentHistories { get; set; } = null!;
        public virtual DbSet<Point> Points { get; set; } = null!;
        public virtual DbSet<PointTransaction> PointTransactions { get; set; } = null!;
        public virtual DbSet<Post> Posts { get; set; } = null!;
        public virtual DbSet<Role> Roles { get; set; } = null!;
        public virtual DbSet<SystemConfiguration> SystemConfigurations { get; set; } = null!;
        public virtual DbSet<SystemMessage> SystemMessages { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<UserProfile> UserProfiles { get; set; } = null!;
        public virtual DbSet<UserRole> UserRoles { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var ConnectionString = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetConnectionString("DefaultConnection");
                optionsBuilder.UseSqlServer(ConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Auction>(entity =>
            {
                entity.Property(e => e.AuctionId).HasColumnName("AuctionID");

                entity.Property(e => e.CategoryId).HasColumnName("CategoryID");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Currency)
                    .HasMaxLength(10)
                    .HasDefaultValueSql("('VND')");

                entity.Property(e => e.DepositAmount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.DepositDeadline).HasColumnType("datetime");

                entity.Property(e => e.EndTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.IsPrivate).HasDefaultValueSql("((0))");

                entity.Property(e => e.ProductName).HasMaxLength(255);

                entity.Property(e => e.StartTime).HasColumnType("datetime");

                entity.Property(e => e.StartingPrice).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.StepPrice).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.AuctionCategories)
                    .HasForeignKey(d => d.CategoryId)
                    .HasConstraintName("FK__Auctions__Catego__2EDAF651");

                entity.HasOne(d => d.ModeNavigation)
                    .WithMany(p => p.AuctionModeNavigations)
                    .HasForeignKey(d => d.Mode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Auctions__Mode__2FCF1A8A");

                entity.HasOne(d => d.PaymentStatusNavigation)
                    .WithMany(p => p.AuctionPaymentStatusNavigations)
                    .HasForeignKey(d => d.PaymentStatus)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Auctions__Paymen__30C33EC3");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Auctions)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__Auctions__UserID__31B762FC");
            });

            modelBuilder.Entity<AuctionBid>(entity =>
            {
                entity.HasKey(e => e.BidId)
                    .HasName("PK__AuctionB__4A733DB2EB2B39F2");

                entity.Property(e => e.BidId).HasColumnName("BidID");

                entity.Property(e => e.AuctionId).HasColumnName("AuctionID");

                entity.Property(e => e.BidAmount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.BidTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Currency)
                    .HasMaxLength(10)
                    .HasDefaultValueSql("('VND')");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.Auction)
                    .WithMany(p => p.AuctionBids)
                    .HasForeignKey(d => d.AuctionId)
                    .HasConstraintName("FK__AuctionBi__Aucti__2645B050");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AuctionBids)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__AuctionBi__UserI__2739D489");
            });

            modelBuilder.Entity<AuctionImage>(entity =>
            {
                entity.HasKey(e => e.ImageId)
                    .HasName("PK__AuctionI__7516F4EC6CAC2918");

                entity.Property(e => e.ImageId).HasColumnName("ImageID");

                entity.Property(e => e.AuctionId).HasColumnName("AuctionID");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ImageUrl).HasColumnName("ImageURL");

                entity.Property(e => e.SortOrder).HasDefaultValueSql("((0))");

                entity.HasOne(d => d.Auction)
                    .WithMany(p => p.AuctionImages)
                    .HasForeignKey(d => d.AuctionId)
                    .HasConstraintName("FK__AuctionIm__Aucti__282DF8C2");
            });

            modelBuilder.Entity<AuctionInvitation>(entity =>
            {
                entity.HasKey(e => e.InvitationId)
                    .HasName("PK__AuctionI__033C8D2F6627A1C0");

                entity.Property(e => e.InvitationId).HasColumnName("InvitationID");

                entity.Property(e => e.AcceptedAt).HasColumnType("datetime");

                entity.Property(e => e.AuctionId).HasColumnName("AuctionID");

                entity.Property(e => e.InvitedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.InvitedUserId).HasColumnName("InvitedUserID");

                entity.Property(e => e.IsAccepted).HasDefaultValueSql("((0))");

                entity.HasOne(d => d.Auction)
                    .WithMany(p => p.AuctionInvitations)
                    .HasForeignKey(d => d.AuctionId)
                    .HasConstraintName("FK__AuctionIn__Aucti__29221CFB");

                entity.HasOne(d => d.InvitedUser)
                    .WithMany(p => p.AuctionInvitations)
                    .HasForeignKey(d => d.InvitedUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__AuctionIn__Invit__2A164134");
            });

            modelBuilder.Entity<AuctionPost>(entity =>
            {
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Auction)
                    .WithMany(p => p.AuctionPosts)
                    .HasForeignKey(d => d.AuctionId)
                    .HasConstraintName("FK_AuctionPosts_Auctions");

                entity.HasOne(d => d.Post)
                    .WithMany(p => p.AuctionPosts)
                    .HasForeignKey(d => d.PostId)
                    .HasConstraintName("FK_AuctionPosts_Posts");
            });

            modelBuilder.Entity<AuctionRequest>(entity =>
            {
                entity.HasKey(e => e.RequestId)
                    .HasName("PK__AuctionR__33A8519A17FAB59E");

                entity.Property(e => e.RequestId).HasColumnName("RequestID");

                entity.Property(e => e.ApprovedAt).HasColumnType("datetime");

                entity.Property(e => e.AuctionId).HasColumnName("AuctionID");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.Auction)
                    .WithMany(p => p.AuctionRequests)
                    .HasForeignKey(d => d.AuctionId)
                    .HasConstraintName("FK__AuctionRe__Aucti__2B0A656D");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AuctionRequests)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__AuctionRe__UserI__2BFE89A6");
            });

            modelBuilder.Entity<AuctionReview>(entity =>
            {
                entity.HasKey(e => e.ReviewId)
                    .HasName("PK__AuctionR__74BC79AE80EDC4B8");

                entity.HasIndex(e => new { e.AuctionId, e.UserId }, "UQ_Auction_User")
                    .IsUnique();

                entity.Property(e => e.ReviewId).HasColumnName("ReviewID");

                entity.Property(e => e.AuctionId).HasColumnName("AuctionID");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.Auction)
                    .WithMany(p => p.AuctionReviews)
                    .HasForeignKey(d => d.AuctionId)
                    .HasConstraintName("FK__AuctionRe__Aucti__2CF2ADDF");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AuctionReviews)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__AuctionRe__UserI__2DE6D218");
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.LogId)
                    .HasName("PK__AuditLog__5E5499A880DD81F1");

                entity.Property(e => e.LogId).HasColumnName("LogID");

                entity.Property(e => e.Action).HasMaxLength(255);

                entity.Property(e => e.Timestamp)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AuditLogs)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__AuditLogs__UserI__32AB8735");
            });

            modelBuilder.Entity<AutoBid>(entity =>
            {
                entity.Property(e => e.AutoBidId).HasColumnName("AutoBidID");

                entity.Property(e => e.AuctionId).HasColumnName("AuctionID");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Currency)
                    .HasMaxLength(10)
                    .HasDefaultValueSql("('VND')");

                entity.Property(e => e.CurrentBidAmount)
                    .HasColumnType("decimal(18, 2)")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.MaxBidAmount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.Auction)
                    .WithMany(p => p.AutoBids)
                    .HasForeignKey(d => d.AuctionId)
                    .HasConstraintName("FK__AutoBids__Auctio__339FAB6E");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(e => e.CategoryId).HasColumnName("CategoryID");

                entity.Property(e => e.CategoryName).HasMaxLength(255);

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Value)
                    .HasColumnType("decimal(18, 2)")
                    .HasDefaultValueSql("((0.00))");
            });

            modelBuilder.Entity<Chat>(entity =>
            {
                entity.Property(e => e.ChatId).HasColumnName("ChatID");

                entity.Property(e => e.AuctionId).HasColumnName("AuctionID");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsGroupChat).HasDefaultValueSql("((0))");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Auction)
                    .WithMany(p => p.Chats)
                    .HasForeignKey(d => d.AuctionId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__Chats__AuctionID__3864608B");
            });

            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(e => e.MessageId)
                    .HasName("PK__ChatMess__C87C037C0021FFAA");

                entity.Property(e => e.MessageId).HasColumnName("MessageID");

                entity.Property(e => e.ChatId).HasColumnName("ChatID");

                entity.Property(e => e.IsDeleted).HasDefaultValueSql("((0))");

                entity.Property(e => e.SenderId).HasColumnName("SenderID");

                entity.Property(e => e.SentAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Chat)
                    .WithMany(p => p.ChatMessages)
                    .HasForeignKey(d => d.ChatId)
                    .HasConstraintName("FK__ChatMessa__ChatI__3493CFA7");

                entity.HasOne(d => d.Sender)
                    .WithMany(p => p.ChatMessages)
                    .HasForeignKey(d => d.SenderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ChatMessa__Sende__3587F3E0");
            });

            modelBuilder.Entity<ChatParticipant>(entity =>
            {
                entity.HasKey(e => new { e.ChatId, e.UserId })
                    .HasName("PK__ChatPart__78836AEC601CFF72");

                entity.Property(e => e.ChatId).HasColumnName("ChatID");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.Property(e => e.JoinedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Chat)
                    .WithMany(p => p.ChatParticipants)
                    .HasForeignKey(d => d.ChatId)
                    .HasConstraintName("FK__ChatParti__ChatI__367C1819");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.ChatParticipants)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ChatParti__UserI__37703C52");
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            });

            modelBuilder.Entity<Deposit>(entity =>
            {
                entity.Property(e => e.DepositId).HasColumnName("DepositID");

                entity.Property(e => e.AuctionId).HasColumnName("AuctionID");

                entity.Property(e => e.Currency)
                    .HasMaxLength(10)
                    .HasDefaultValueSql("('VND')");

                entity.Property(e => e.DepositAmount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.DepositDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DepositStatus).HasMaxLength(50);

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Deposits)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__Deposits__UserID__395884C4");
            });

            modelBuilder.Entity<Dispute>(entity =>
            {
                entity.Property(e => e.DisputeId).HasColumnName("DisputeID");

                entity.Property(e => e.AuctionId).HasColumnName("AuctionID");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatorId).HasColumnName("CreatorID");

                entity.Property(e => e.DisputeStatusId).HasColumnName("DisputeStatusID");

                entity.Property(e => e.ResolvedAt).HasColumnType("datetime");

                entity.Property(e => e.WinnerId).HasColumnName("WinnerID");

                entity.HasOne(d => d.Auction)
                    .WithMany(p => p.Disputes)
                    .HasForeignKey(d => d.AuctionId)
                    .HasConstraintName("FK__Disputes__Auctio__3A4CA8FD");

                entity.HasOne(d => d.Creator)
                    .WithMany(p => p.DisputeCreators)
                    .HasForeignKey(d => d.CreatorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Disputes__Creato__3B40CD36");

                entity.HasOne(d => d.DisputeStatus)
                    .WithMany(p => p.Disputes)
                    .HasForeignKey(d => d.DisputeStatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Disputes__Disput__3C34F16F");

                entity.HasOne(d => d.Winner)
                    .WithMany(p => p.DisputeWinners)
                    .HasForeignKey(d => d.WinnerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Disputes__Winner__3D2915A8");
            });

            modelBuilder.Entity<EvidenceFile>(entity =>
            {
                entity.HasKey(e => e.EvidenceId)
                    .HasName("PK__Evidence__FA39D78DF67CB6C9");

                entity.Property(e => e.EvidenceId).HasColumnName("EvidenceID");

                entity.Property(e => e.DisputeId).HasColumnName("DisputeID");

                entity.Property(e => e.FileType).HasMaxLength(50);

                entity.Property(e => e.FileUrl).HasColumnName("FileURL");

                entity.Property(e => e.UploadedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Dispute)
                    .WithMany(p => p.EvidenceFiles)
                    .HasForeignKey(d => d.DisputeId)
                    .HasConstraintName("FK__EvidenceF__Dispu__3E1D39E1");

                entity.HasOne(d => d.UploadedByNavigation)
                    .WithMany(p => p.EvidenceFiles)
                    .HasForeignKey(d => d.UploadedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__EvidenceF__Uploa__3F115E1A");
            });

            modelBuilder.Entity<Like>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.Property(e => e.NotificationId).HasColumnName("NotificationID");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.IsRead).HasDefaultValueSql("((0))");

                entity.Property(e => e.SentAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Title).HasMaxLength(255);

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__Notificat__UserI__489AC854");
            });

            modelBuilder.Entity<PasswordResetRequest>(entity =>
            {
                entity.HasKey(e => e.RequestId)
                    .HasName("PK__Password__33A8519A723801E4");

                entity.Property(e => e.RequestId).HasColumnName("RequestID");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Email).HasMaxLength(255);

                entity.Property(e => e.ExpiresAt).HasColumnType("datetime");

                entity.Property(e => e.IsUsed).HasDefaultValueSql("((0))");

                entity.Property(e => e.ResetToken).HasMaxLength(255);

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.PasswordResetRequests)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__PasswordR__UserI__40F9A68C");
            });

            modelBuilder.Entity<PaymentHistory>(entity =>
            {
                entity.HasKey(e => e.PaymentId)
                    .HasName("PK__PaymentH__9B556A58257B5768");

                entity.ToTable("PaymentHistory");

                entity.Property(e => e.PaymentId).HasColumnName("PaymentID");

                entity.Property(e => e.AccountNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.AuctionId).HasColumnName("AuctionID");

                entity.Property(e => e.Currency)
                    .HasMaxLength(10)
                    .HasDefaultValueSql("('VND')");

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.PaymentAmount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.PaymentCode)
                    .HasMaxLength(16)
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.PaymentTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.PaymentHistories)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__PaymentHi__UserI__41EDCAC5");
            });

            modelBuilder.Entity<Point>(entity =>
            {
                entity.Property(e => e.PointId).HasColumnName("PointID");

                entity.Property(e => e.Currency)
                    .HasMaxLength(10)
                    .HasDefaultValueSql("('VND')");

                entity.Property(e => e.LastUpdated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.PointsAmount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Points)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__Points__UserID__42E1EEFE");
            });

            modelBuilder.Entity<PointTransaction>(entity =>
            {
                entity.HasKey(e => e.TransactionId)
                    .HasName("PK__PointTra__55433A4B4A2B6FB7");

                entity.Property(e => e.TransactionId).HasColumnName("TransactionID");

                entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Currency)
                    .HasMaxLength(10)
                    .HasDefaultValueSql("('VND')");

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.TransactionCode).HasMaxLength(16);

                entity.Property(e => e.TransactionTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.PointTransactions)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__PointTran__UserI__43D61337");
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RoleName).HasMaxLength(50);

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<SystemConfiguration>(entity =>
            {
                entity.HasKey(e => e.ConfigId)
                    .HasName("PK__SystemCo__C3BC333CCC5DDF24");

                entity.Property(e => e.ConfigId).HasColumnName("ConfigID");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.KeyId)
                    .HasMaxLength(255)
                    .HasColumnName("KeyID");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Value).HasMaxLength(255);
            });

            modelBuilder.Entity<SystemMessage>(entity =>
            {
                entity.HasKey(e => e.MessageId)
                    .HasName("PK__SystemMe__C87C037C0305D6F6");

                entity.HasIndex(e => e.Code, "UQ__SystemMe__A25C5AA7AC844E36")
                    .IsUnique();

                entity.Property(e => e.MessageId).HasColumnName("MessageID");

                entity.Property(e => e.Code)
                    .HasMaxLength(4)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.LanguageCode)
                    .HasMaxLength(10)
                    .HasDefaultValueSql("('vi')");

                entity.Property(e => e.Type)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.EmailVerified).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.LastLogin).HasColumnType("datetime");

                entity.Property(e => e.Password).HasMaxLength(255);

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Username).HasMaxLength(100);

                entity.Property(e => e.VerifyToken).HasMaxLength(255);

                entity.Property(e => e.VerifyTokenExpiresAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.HasKey(e => e.UserId)
                    .HasName("PK__UserProf__1788CCAC10B41C50");

                entity.HasIndex(e => e.Email, "UQ__UserProf__A9D105344B51DD4A")
                    .IsUnique();

                entity.Property(e => e.UserId)
                    .ValueGeneratedNever()
                    .HasColumnName("UserID");

                entity.Property(e => e.Address).HasMaxLength(500);

                entity.Property(e => e.Cccd)
                    .HasMaxLength(50)
                    .HasColumnName("CCCD");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Dob)
                    .HasColumnType("date")
                    .HasColumnName("DOB");

                entity.Property(e => e.Email).HasMaxLength(255);

                entity.Property(e => e.FullName).HasMaxLength(255);

                entity.Property(e => e.PhoneNumber).HasMaxLength(20);

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.User)
                    .WithOne(p => p.UserProfile)
                    .HasForeignKey<UserProfile>(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__UserProfi__UserI__44CA3770");
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId })
                    .HasName("PK__UserRole__AF27604F34EE5D0F");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity.Property(e => e.AssignedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK__UserRoles__RoleI__45BE5BA9");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__UserRoles__UserI__46B27FE2");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
