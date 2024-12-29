using BE_AuctionAOT.Common.Base.Entity;
using System.ComponentModel.DataAnnotations;

namespace BE_AuctionAOT.DAO.AuctionManagement.Dispute
{
	public class ListAuctionBiddedInputDto : BaseInputDto
	{
		public string searchText { get; set; }

		public DateOnly? startDate { get; set; }
		public DateOnly? endDate { get; set; }
	}
	public class WinnerAuctionInputDto : BaseInputDto
	{
		public string searchText { get; set; }
		public DateOnly? startDate { get; set; }
		public DateOnly? endDate { get; set; }
	}
	public class ListDisputeInputDto : BaseInputDto
	{
		public string searchText { get; set; }
		public DateOnly? startDate { get; set; }
		public DateOnly? endDate { get; set; }
	}
	public class AdminDecisionInputDto
	{
		[Required]
		public long DisputeID { get; set; }
		[Required]
		public bool IsCreatorTrue { get; set; }
		[Required]
		public string Decision { get; set; }
	}
    public class CustomerPaymentConfirmationInputDto
    {
		[Required]
        public long AuctionId { get; set; }
        [Required]
        public bool IsPaid { get; set; }
		public string? CreatorEvidence { get; set; }
        public string? WinnerEvidence { get; set; }

        public IFormFile? FileEvidence { get; set; }
    }
}
