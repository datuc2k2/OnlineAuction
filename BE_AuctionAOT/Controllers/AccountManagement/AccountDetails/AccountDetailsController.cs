using BE_AuctionAOT.Common.Constants;
using BE_AuctionAOT.DAO.AccountManagement.Account;
using BE_AuctionAOT.DAO.AccountManagement.Account_Details;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BE_AuctionAOT.Controllers.AccountManagement.Account_Details
{
	[Route("api/[controller]")]
	[ApiController]
	public class AccountDetailsController : ControllerBase
	{
		private readonly AccountDetailDao _accountDao;
		public AccountDetailsController(AccountDetailDao accountDao)
		{
			_accountDao = accountDao;

		}
		[Authorize]
		[HttpPost]
		public async Task<IActionResult> getAccountDetail(AccountDetailDaoInputDto Input)
		{
			try
			{

				var accountDetail = new AccountDetailDaoOutputDto();
				accountDetail = await _accountDao.GetAccount(Input);
				if (accountDetail.ResultCd == ResultCd.FAILURE)
				{
					return BadRequest(accountDetail);
				};
				return Ok(accountDetail);
			}
			catch (Exception ex)
			{
				return BadRequest();
			}
		}
		[Authorize]
		[HttpPost("Edit")]
		public async Task<IActionResult> editAccountDetail(AccountDetailDaoInputDto Input)
		{
			try
			{

				var accountEdit = new AccountDetailDaoOutputDto();
				accountEdit = await _accountDao.EditAccountdetail(Input);
				if (accountEdit.ResultCd == ResultCd.FAILURE)
				{
					return BadRequest(accountEdit);
				};
				return Ok(accountEdit);
			}
			catch (Exception ex)
			{
				return BadRequest();
			}
		}
	}
}
