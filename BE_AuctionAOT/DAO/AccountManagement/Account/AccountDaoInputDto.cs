using BE_AuctionAOT.Common.Base.Entity;

namespace BE_AuctionAOT.DAO.AccountManagement.Account
{

	public class AccountDaoInputDto : BaseInputDto
	{
		public string searchUserName { get; set; }
		public string searchEmail { get; set; }
		public DateOnly? startDate { get; set; }
		public DateOnly? endDate { get; set; }
		public bool? status { get; set; }

	}
}
