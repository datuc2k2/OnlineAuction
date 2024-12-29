using BE_AuctionAOT.Common.Base.Entity;

namespace BE_AuctionAOT.DAO.AuctionManagement.ListAuction
{
	
	public class ListAuctionOutputDto : BaseOutputDto
	{
		public List<AuctionDto> Auction { get; set; }
		public int allRecords { get; set; }
	}
	public class JoinedAuctionOutputDto : BaseOutputDto
	{
		public List<AuctionBidDto> auctionBidDtos { get; set; }
		public int allRecords { get; set; }
	}
	public class GetStatusOutputDto : BaseOutputDto
	{
		public List<Dropdown>? Status { get; set; }
		public List<Dropdown>? PaymentStatus { get; set; }
	}

	public class AuctionDto
	{
		public long AuctionId { get; set; }
		public long UserId { get; set; }
		public string ProductName { get; set; } = null!;
		//them status cho auction
		public long Status { get; set; }
		//them status Name
		public string StatusName { get; set; } = null!;
		public long PaymentStatus { get; set; }
		public long CategoryId { get; set; }
		//them fee
		public decimal? Fee { get; set; } = null!;
		public decimal StartingPrice { get; set; }
		public string Currency { get; set; } = null!;
		public decimal StepPrice { get; set; }
		public long Mode { get; set; }
		public string Description { get; set; } = null!;
		public bool? IsPrivate { get; set; }
		public decimal? DepositAmount { get; set; }
		public DateTime? DepositDeadline { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public bool? IsActive { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public virtual ICollection<AuctionRequestDto> AuctionRequests { get; set; }
	}
	public class AuctionRequestDto
	{
		public long RequestId { get; set; }
		public long AuctionId { get; set; }
		public long UserId { get; set; }
		public bool Type { get; set; }
		public string? RequestDetails { get; set; }
		public bool? IsApproved { get; set; }
		public DateTime? ApprovedAt { get; set; }
	}
	public class AuctionBidDto
	{
		public long AuctionId { get; set; }
		public long UserId { get; set; }
		public decimal BidAmount { get; set; }
		public string Currency { get; set; } = null!;
		public DateTime? BidTime { get; set; }

		public virtual JoinedAuctionDto Auction { get; set; } = null!;
	}
	public class JoinedAuctionDto
	{
		public long UserId { get; set; }
		public string ProductName { get; set; } = null!;
		public long Status { get; set; }
		//them status Name
		public string StatusName { get; set; } = null!;
		public long CategoryId { get; set; }
		//them ten loai san pham
		public string CategoryName { get; set; }
		//them gia ket thuc
		public decimal EndPrice { get; set; }
		public string Currency { get; set; } = null!;
		public decimal StepPrice { get; set; }
		public long Mode { get; set; }
		public bool? IsPrivate { get; set; }
		public DateTime EndTime { get; set; }
		public bool? IsActive { get; set; }
		//them trạng thái có thắng không
		public bool? isWinner { get; set; }
	}
	public class AuctionInputDto : BaseOutputDto
	{
		public List<Models.Auction> Auctions { get; set; } = new List<Models.Auction>();
		public int TotalRecords { get; set; }
    }
}
