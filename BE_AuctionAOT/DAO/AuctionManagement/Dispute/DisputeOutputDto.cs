using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.Models;

namespace BE_AuctionAOT.DAO.AuctionManagement.Dispute
{
	public class JoinedAuctionOutputDto : BaseOutputDto
	{
		public List<AuctionBidDto> auctionBidDtos { get; set; }
		public int allRecords { get; set; }
	}
	public class ListBidedOutputDto : BaseOutputDto
	{
		public List<AuctionBidedDto> Auction { get; set; }
		public int allRecords { get; set; }
	}
	public class ListDisputeOutputDto : BaseOutputDto
	{
		public List<DisputeDto> disputeDtos { get; set; }
		public int allRecords { get; set; }
	}
	public class DisputeDetailDto : BaseOutputDto
	{
		public DisputeDto disputeDto { get; set; }
		public EvidenceFileDto winnerEvidence { get; set; }
		public EvidenceFileDto creatorEvidence { get; set; }

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
	//list dung cho phia ben confirm
	public class AuctionBidedDto
	{
		public long AuctionId { get; set; }
		public long UserId { get; set; }
		public string ProductName { get; set; } = null!;
		public long CategoryId { get; set; }
		//them ten loai san pham
		public string CategoryName { get; set; }
		//them gia ket thuc
		public decimal EndPrice { get; set; }
		public string Currency { get; set; } = "VND"!;
		public DateTime? DepositDeadline { get; set; }

		public DateTime EndTime { get; set; }
		public long WinnerID { get; set; }
		public string WinnerName { get; set; }
		public string confirmStatus { get; set; }
		public string depositDlStatus { get; set; }

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
		public string confirmStatus { get; set; }
		public string depositDlStatus { get; set; }
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
		public DateTime? DepositDeadline { get; set; }
		public DateTime EndTime { get; set; }
		public bool? IsActive { get; set; }
		//them trạng thái có thắng không
		public bool? isWinner { get; set; }
	}

	public class DisputeDto
	{
		public long DisputeId { get; set; }
		public long AuctionId { get; set; }
		public long WinnerId { get; set; }
		public string WinnerName { get; set; }
		public long CreatorId { get; set; }
		public string CreatorName { get; set; }
		public string? DisputeReason { get; set; }
		public bool? WinnerConfirmed { get; set; }
		public bool? CreatorConfirmed { get; set; }
		public string? WinnerEvidence { get; set; }
		public string? CreatorEvidence { get; set; }
		public string? AdminDecision { get; set; }
		public long DisputeStatusId { get; set; }
		public DateTime? CreatedAt { get; set; }
		public virtual AuctionBidedDto Auction { get; set; } = null!;
	}
    public class CustomerPaymentConfirmationOutputDto : BaseOutputDto
    {
        public Models.Dispute? Dispute { get; set; }
    }
	public class EvidenceFileDto
	{
		public long EvidenceId { get; set; }
		public long DisputeId { get; set; }
		public long UploadedBy { get; set; }
		public string FileUrl { get; set; } = null!;
		public string? FileType { get; set; }
		public DateTime? UploadedAt { get; set; }
	}
}
