using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.Common.Constants;
using BE_AuctionAOT.Common.Utility;
using BE_AuctionAOT.DAO.AuctionManagement.AuctionInvitation;
using BE_AuctionAOT.DAO.AuctionManagement.AuctionRequest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BE_AuctionAOT.Controllers.AuctionManagement.AuctionRequest
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuctionRequestController : ControllerBase
	{
		private readonly AuctionRequestDao _auctionRequestDao;
		private readonly AuthUtility _authUtility;
		public AuctionRequestController(AuctionRequestDao auctionRequestDao, AuthUtility authUtility)
		{
			_auctionRequestDao = auctionRequestDao;
			_authUtility = authUtility;
		}

		[Authorize]
		[HttpPost("List")]
		public async Task<IActionResult> GetListAuctionRequest(AuctionRequestInputDto inputDto)
		{
			try
			{
				var listAuctionRequest = new AuctionRequestOutputDto();
				listAuctionRequest = await _auctionRequestDao.GetListAuctionRequest(inputDto);
				if (listAuctionRequest.ResultCd == ResultCd.FAILURE)
				{
					return BadRequest(listAuctionRequest);
				};

				return Ok(listAuctionRequest);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		[Authorize]
		[HttpPost("IsAccepted")]
		public async Task<IActionResult> IsAcceptedRequest(IsAcceptedAucRequestInputDto inputDto)
		{
			try
			{
				var accepted = new BaseOutputDto();
				accepted = await _auctionRequestDao.IsAcceptedRequest(inputDto);
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
