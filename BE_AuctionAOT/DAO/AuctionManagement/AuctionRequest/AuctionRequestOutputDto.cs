using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.Models;

namespace BE_AuctionAOT.DAO.AuctionManagement.AuctionRequest
{
	public class AuctionRequestOutputDto : BaseOutputDto
	{
		public List<AuctionRequestFromDto> AuctionRequests { get; set; }
		public int allRecords {  get; set; }
	}

	public class AuctionDto
	{
		public string ProductName { get; set; } = null!;
		public long CategoryId { get; set; }
		public string CategoryName { get; set; }
		public decimal StartingPrice { get; set; }
		public string Currency { get; set; } = null!;
		public decimal StepPrice { get; set; }
		public long Mode { get; set; }
		public string ModeName { get; set; }
		public string Description { get; set; } = null!;
		public bool? IsPrivate { get; set; }
		public decimal? DepositAmount { get; set; }
		public DateTime? DepositDeadline { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public bool? IsActive { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public long Status { get; set; }
		public long PaymentStatus { get; set; }

		public virtual ICollection<AuctionImageDto> AuctionImages { get; set; }
	}
	public class UserDto
	{
		public string Username { get; set; } = null!;
		public virtual UserProfileDto? UserProfile { get; set; }
	}
	public partial class UserProfileDto
	{
		public string? Avatar { get; set; }
	}
		public partial class AuctionImageDto
	{
		public string ImageUrl { get; set; } = null!;
	}
	public partial class AuctionRequestDto
	{
		public long RequestId { get; set; }
		public long AuctionId { get; set; }
		public long UserId { get; set; }
		public bool Type { get; set; }
		public string? RequestDetails { get; set; }
		public bool? IsApproved { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? ApprovedAt { get; set; }
		public long? ApprovedBy { get; set; }
		public virtual UserDto User { get; set; } = null!;
		public virtual AuctionDto Auction { get; set; } = null!;		
	}
	public class AuctionRequestFromDto
	{
		public long RequestId { get; set; }
		public long AuctionId { get; set; }
		public long UserId { get; set; }
		public bool Type { get; set; }
		public string? RequestDetails { get; set; }
		public bool? IsApproved { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? ApprovedAt { get; set; }
		public long? ApprovedBy { get; set; }
		public UserDto User { get; set; } = null!;
		public AuctionDto Auction { get; set; } = null!;

		//public string Username { get; set; } = null!;
		//public string? Avatar { get; set; }

		//public string ProductName { get; set; } = null!;
		//public Dropdown? AuCategory { get; set; }
		//public decimal StartingPrice { get; set; }
		//public string Currency { get; set; } = null!;
		//public decimal StepPrice { get; set; }
		//public Dropdown? AucMode { get; set; }
		//public string Description { get; set; } = null!;
		//public bool? IsPrivate { get; set; }
		//public decimal? DepositAmount { get; set; }
		//public DateTime? DepositDeadline { get; set; }
		//public DateTime StartTime { get; set; }
		//public DateTime EndTime { get; set; }
		//public List<string>? Images { get; set; }

	}
}
