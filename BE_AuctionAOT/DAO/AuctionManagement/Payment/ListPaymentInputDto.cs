using BE_AuctionAOT.Common.Base.Entity;

namespace BE_AuctionAOT.DAO.AuctionManagement.Payment
{
	public class ListPaymentInputDto : BaseInputDto
	{
		public String searchName { get; set; }
		public DateOnly? startDate { get; set; }
		public DateOnly? endDate { get; set; }
	}
}
