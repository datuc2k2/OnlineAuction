using BE_AuctionAOT.Common.Constants;
using BE_AuctionAOT.DAO.AccountManagement.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BE_AuctionAOT.Controllers.AccountManagement.Account
{
	[Route("api/[controller]")]
	[ApiController]
	public class AccountController : ControllerBase
	{
		private readonly AccountDao _accountDao;
		public AccountController(AccountDao accountDao)
		{
			_accountDao = accountDao;

		}

		[Authorize]
		[HttpPost("GetAccounts")]
		public async Task<IActionResult> getAccountList(AccountDaoInputDto Input)
		{
			try
			{
				
				var accountList = new AccountDaoOutputDto();
				accountList = await _accountDao.GetAccountList(Input);
				if (accountList.ResultCd == ResultCd.FAILURE)
				{
					return BadRequest(accountList);
				};
				return Ok(accountList);
			}
			catch (Exception ex)
			{
				return BadRequest();
			}
		}

		[Authorize]
		[HttpGet("GetAccountInvites")]
		public async Task<IActionResult> getAccountInvites()
		{
			try
			{

				var accInviList = new AccountInviteDaoOutputDto();
				accInviList = await _accountDao.GetAccountInvite();
				if (accInviList.ResultCd == ResultCd.FAILURE)
				{
					return BadRequest(accInviList);
				};
				return Ok(accInviList);
			}
			catch (Exception ex)
			{
				return BadRequest();
			}
		}

		[HttpGet("OtherUserInfor/{UserID}")]
		public async Task<IActionResult> getOtherUserInfor(int UserID)
		{
			try
			{
				var otherUserInfor = new OtherUserInforOutputDto();
				otherUserInfor = await _accountDao.GetOtherUserInfor(UserID);
				if (otherUserInfor == null)
				{
					return BadRequest(otherUserInfor);
				};
				return Ok(otherUserInfor);
			}
			catch (Exception ex)
			{
				return BadRequest();
			}
		}


	}
	
}
