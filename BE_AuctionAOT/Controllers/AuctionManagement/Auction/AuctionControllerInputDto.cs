using System.ComponentModel.DataAnnotations;

namespace BE_AuctionAOT.Controllers.AuctionManagement.Auction
{
    public class UpdateAuctionInputDto
    {
        [Required]
        public UpdateAuction UpdateAuction { get; set; }
        public List<IFormFile> FilesToAdd { get; set; }
        [Required]
        public List<string> ExistingBlobNames { get; set; }
    }
    public class CreateAuctionInputDto
    {
        [Required]
        public Auction Auction { get; set; }
        [Required]
        public List<IFormFile> Images { get; set; }
    }
    public class Auction
    {
        [Required]
        public long UserID { get; set; }
        [Required]
        public string ProductName { get; set; } = null!;
        [Required]
        public long? CategoryId { get; set; }
        [Required]
        public decimal StartingPrice { get; set; }
        [Required]
        public string Currency { get; set; } = null!;
        [Required]
        public decimal StepPrice { get; set; }
        [Required]
        public long? Mode { get; set; }
        [Required]
        public string Description { get; set; } = null!;
        [Required]
        public bool? IsPrivate { get; set; }
        public List<long>? InvitedIds { get; set; }
        [Required]
        public decimal? DepositAmount { get; set; }
        [Required]
        public DateTime? DepositDeadline { get; set; }
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public DateTime EndTime { get; set; }
    }

    public class UpdateAuction : Auction
    {
        [Required]
        public long AuctionID { get; set; }
    }
}
