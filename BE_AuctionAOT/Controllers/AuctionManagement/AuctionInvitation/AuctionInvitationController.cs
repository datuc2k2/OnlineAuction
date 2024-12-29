using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.Common.Constants;
using BE_AuctionAOT.Common.Utility;
using BE_AuctionAOT.DAO.AuctionManagement.AuctionInvitation;
using BE_AuctionAOT.DAO.AuctionManagement.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BE_AuctionAOT.Controllers.AuctionManagement.AuctionInvitation
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuctionInvitationController : ControllerBase
	{
		private readonly AuctionInvitationDao _auctionInvitationDao;
		private readonly AuthUtility _authUtility;
		public AuctionInvitationController(AuctionInvitationDao auctionInvitationDao, AuthUtility authUtility)
		{
			_auctionInvitationDao = auctionInvitationDao;
			_authUtility = authUtility;
		}

		[Authorize]
		[HttpPost("List")]
		public async Task<IActionResult> GetListAuctionInvitation(AuctionInvitationInputDto inputDto)
		{
			try
			{
				//String userId = User.Claims.FirstOrDefault(claim => claim.Type == "ID")?.Value;
				var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
				var uId = _authUtility.GetIdInHeader(token);
				var listAuctionInvitation = new AuctionInvitationOutputDto();
				listAuctionInvitation = await _auctionInvitationDao.getListAuctionInvitation(uId, inputDto);
				if (listAuctionInvitation.ResultCd == ResultCd.FAILURE)
				{
					return BadRequest(listAuctionInvitation);
				};

				return Ok(listAuctionInvitation);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}
		[Authorize]
		[HttpPost("IsAccepted")]
		public async Task<IActionResult> IsAcceptAuctionInvitation(IsAcceptedInvitation inputDto)
		{
			try
			{
				var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
				var uId = _authUtility.GetIdInHeader(token);
				var accepted = new BaseOutputDto();
				accepted = await _auctionInvitationDao.isAccepttAuctionInvitation(uId, inputDto);
				if (accepted.ResultCd == ResultCd.FAILURE)
				{
					return BadRequest(accepted);
				};

				return Ok(accepted);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}
	}
}
