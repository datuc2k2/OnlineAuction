namespace BE_AuctionAOT.DAO.AuctionManagement.Payment
{
	public class DepositInputDto
	{
		public long AuctionId { get; set; }
		public long UserId { get; set; }
		public decimal DepositAmount { get; set; }
		public string Currency { get; set; } = null!;
	}
}
