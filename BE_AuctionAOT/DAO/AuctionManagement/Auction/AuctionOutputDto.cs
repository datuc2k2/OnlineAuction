using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.Models;
using System.ComponentModel.DataAnnotations;

namespace BE_AuctionAOT.DAO.AuctionManagement.Auction
{

    public class CreateAuctionOutputDto : BaseOutputDto
    {
        public long NewAucId { get; set; }
    }
    public class GetAuctionCategoryOutputDto : BaseOutputDto
    {
        public List<Dropdown>? Categories { get; set; }
        public List<Dropdown>? Modes { get; set; }
    }
    public class GetCurrentBlobNamesFromDatabaseOutputDto : BaseOutputDto
    {
        public List<string> Names { get; set; }
    }

    public class AuctionDetailsOutputDto : BaseOutputDto
    {
        public Auction? Auction { get; set; }
        public CreaterProfile? createrProfile { get; set; }
        public long? Fee { get; set; }
    }

    public class Auction
    {
        public long UserID { get; set; }
        public string ProductName { get; set; } = null!;
        public long? CategoryId { get; set; }
        public decimal StartingPrice { get; set; }
        public string Currency { get; set; } = null!;
        public decimal StepPrice { get; set; }
        public Dropdown? AucStatus { get; set; }
        public Dropdown? AucStatusPayment { get; set; }
        public long? Mode { get; set; }
        public string Description { get; set; } = null!;
        public bool? IsPrivate { get; set; }
        public List<long>? InvitedIds { get; set; }
        public decimal? DepositAmount { get; set; }
        public DateTime? DepositDeadline { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<string>? Images { get; set; }
    }
    public class CreaterProfile
    {
        public string? Name { get; set; }
        public string? Avata { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Location { get; set; }
    }
}
