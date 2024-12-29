using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.Models;

namespace BE_AuctionAOT.DAO.AuctionManagement.AuctionInvitation
{
    public class AuctionInvitationOutputDto : BaseOutputDto
    {
        public List<AuctionInvitationDto> auctionInvitations {  get; set; }
        public int allRecords { get; set; }
    }
    public class CreateInvitationOutputDto : BaseOutputDto
    {
    }
    public class GetCurInvitationOutputDto : BaseOutputDto
    {
        public List<long> Invs { get; set; }
    }

	public class AuctionInvitationDto
	{
		public long InvitationId { get; set; }
		public long AuctionId { get; set; }
		public long InvitedUserId { get; set; }
		public bool? IsAccepted { get; set; }
		public DateTime? InvitedAt { get; set; }
		public DateTime? AcceptedAt { get; set; }

		public virtual AuctionDto Auction { get; set; } = null!;
	}
    public class AuctionDto
    {
		public long AuctionId { get; set; }
		public long UserId { get; set; }
		public string ProductName { get; set; } = null!;
		public long CategoryId { get; set; }
		public decimal StartingPrice { get; set; }
		public string Currency { get; set; } = null!;
		public decimal StepPrice { get; set; }
		public long Mode { get; set; }
		public string Description { get; set; } = null!;
		public decimal? DepositAmount { get; set; }
		public DateTime? DepositDeadline { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
	}


}
