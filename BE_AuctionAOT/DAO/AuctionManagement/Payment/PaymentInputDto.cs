namespace BE_AuctionAOT.DAO.AuctionManagement.Payment
{
	public class PaymentInputDto
	{
		public long UserId { get; set; }
		public long? AuctionId { get; set; }
		public decimal PaymentAmount { get; set; }
		public string Currency { get; set; } = null!;
		public DateTime? PaymentTime { get; set; }
		public string? Description { get; set; }
	}
	public class FeeCaculationInputDto
	{
		public long? auctionId { get; set; }
	}
}
