using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.Models;

namespace BE_AuctionAOT.DAO.AccountManagement.Account_Details
{
	public class AccountDetailDaoInputDto
	{
		public string accountId { get; set; }
		public long roleNew {  get; set; }
		public bool IsActive { get; set; }

	}
}
