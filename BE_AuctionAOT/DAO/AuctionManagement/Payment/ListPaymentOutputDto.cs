using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.Models;

namespace BE_AuctionAOT.DAO.AuctionManagement.Payment
{
	public class ListPaymentOutputDto : BaseOutputDto
	{
		public List<PointTransactionDto> PointTransactions {  get; set; }
		public int allRecords { get; set; }
	}

	public class UserProfileDto
	{
		public string? FullName { get; set; }
	}
	public class UserDto
	{
		public long UserId { get; set; }
		public string Username { get; set; } = null!;
		public UserProfileDto? UserProfile { get; set; }
	}

	public class PaymentHistoryDto
	{
		public long PaymentId { get; set; }
		public long UserId { get; set; }
		public long AuctionId { get; set; }
		public decimal PaymentAmount { get; set; }
		public string Currency { get; set; } = null!;
		public DateTime? PaymentTime { get; set; }
		public string? Description { get; set; }

		public UserDto? User { get; set; } // Thêm thông tin của UserDto
	}
	public partial class PointTransactionDto
	{
		public long TransactionId { get; set; }
		public long UserId { get; set; }
		public decimal Amount { get; set; }
		public string Currency { get; set; } = null!;
		public DateTime? TransactionTime { get; set; }
		public string? Description { get; set; }
		public string? TransactionCode { get; set; }

		public UserDto? User { get; set; } = null!;
	}

}
