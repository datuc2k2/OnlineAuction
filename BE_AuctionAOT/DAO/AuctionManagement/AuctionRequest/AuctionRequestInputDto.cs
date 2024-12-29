using BE_AuctionAOT.Common.Base.Entity;

namespace BE_AuctionAOT.DAO.AuctionManagement.AuctionRequest
{
	public class CreateAuctionRequestInputDto
	{
		public long AuctionId { get; set; }
		public long UserId { get; set; }
		public bool Type { get; set; }
		public string RequestDetails { get; set; }
	}

	public class AuctionRequestInputDto : BaseInputDto
	{
		public string searchName { get; set; }
		public bool? isProcessed { get; set; }
		public DateOnly? startDate { get; set; }
		public DateOnly? endDate { get; set; }
	}
	public class IsAcceptedAucRequestInputDto
	{
		public long AuctionId { get; set; }
		public bool IsAccepted { get; set; }
		public string? Reason { get; set; }
	}
}
