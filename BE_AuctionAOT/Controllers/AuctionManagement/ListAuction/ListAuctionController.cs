using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.Common.Constants;
using BE_AuctionAOT.Common.Utility;
using BE_AuctionAOT.DAO.AuctionManagement.ListAuction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BE_AuctionAOT.Controllers.AuctionManagement.ListAuction
{
	[Route("api/[controller]")]
	[ApiController]
	public class ListAuctionController : ControllerBase
	{
		private readonly AuthUtility _authUtility;
		private readonly ListAuctionDao _listAuctionDao;
		public ListAuctionController(AuthUtility authUtility, ListAuctionDao listAuctionDao)
		{
			_authUtility = authUtility;
			_listAuctionDao = listAuctionDao;
		}

		[Authorize]
		[HttpGet("GetStatus")]
		public async Task<IActionResult> GetStatusAuction()
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<GetStatusOutputDto>();
				var status = await _listAuctionDao.GetStatusAuction(2);
				if (status.ResultCd != ResultCd.SUCCESS)
				{
					return BadRequest(status);
				}
				output.Status = status.Status;

				var PaymentStatus = await _listAuctionDao.GetStatusAuction(3);
				if (status.ResultCd != ResultCd.SUCCESS)
				{
					return BadRequest(PaymentStatus);
				}
				output.PaymentStatus = PaymentStatus.Status;
				return Ok(output);

			}
			catch (Exception ex)
			{
				return BadRequest();
			}
		}

		[Authorize]
		[HttpPost("ListMyAuction")]
		public async Task<IActionResult> GetListMyAuction(ListAuctionInputDto inputDto)
		{
			try
			{
				var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
				var uId = _authUtility.GetIdInHeader(token);
				var auctionList = new ListAuctionOutputDto();
				auctionList = await _listAuctionDao.GetListAuction(inputDto, uId.ToString());
				if (auctionList.ResultCd != ResultCd.SUCCESS)
				{
					return BadRequest(auctionList);
				}
				return Ok(auctionList);
			}
			catch (Exception ex)
			{
				return BadRequest();
			}
		}

		[HttpPost("ListOtherUserAuction")]
		public async Task<IActionResult> GetListOtherUserAuction(ListOtherUserAuctionInputDto inputDto)
		{
			try
			{
				ListAuctionInputDto input = new ListAuctionInputDto{
																	DisplayCount = inputDto.DisplayCount,
																	searchText = inputDto.searchText,
																	startDate = inputDto.startDate,
																	endDate = inputDto.endDate,
																	status = inputDto.status,
																};
				var auctionList = new ListAuctionOutputDto();
				auctionList = await _listAuctionDao.GetListAuction(input, inputDto.userId);
				if (auctionList.ResultCd != ResultCd.SUCCESS)
				{
					return BadRequest(auctionList);
				}
				return Ok(auctionList);
			}
			catch (Exception ex)
			{
				return BadRequest();
			}
		}
		[Authorize]
		[HttpPut("InactiveAuction/{AuctionId}")]
		public async Task<IActionResult> InactiveAuction(int AuctionId)
		{
			try
			{
				var inactive = new BaseOutputDto();
				inactive = await _listAuctionDao.InactiveAuction(AuctionId);
				if (inactive.ResultCd != ResultCd.SUCCESS)
				{
					return BadRequest(inactive);
				}
				return Ok(inactive);
			}
			catch (Exception ex)
			{
				return BadRequest();
			}

		}


		[Authorize]
		[HttpPost("ListAuctionAdmin")]
		public async Task<IActionResult> GetListAuctionAdmin(ListAuctionInputDto inputDto)
		{
			try
			{

				var auctionList = new ListAuctionOutputDto();
				auctionList = await _listAuctionDao.GetListAuctionAdmin(inputDto);
				if (auctionList.ResultCd != ResultCd.SUCCESS)
				{
					return BadRequest(auctionList);
				}
				return Ok(auctionList);
			}
			catch (Exception ex)
			{
				return BadRequest();
			}
		}
		[Authorize]
		[HttpPost("ListJoinedAuction")]
		public async Task<IActionResult> GetListJoinedAuctionUser(JoinedAuctionInputDto inputDto)
		{
			try
			{
				var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
				var uId = _authUtility.GetIdInHeader(token);
				var joinedList = new JoinedAuctionOutputDto();
				joinedList = await _listAuctionDao.GetJoinedAuctionList(inputDto, uId);
				if (joinedList.ResultCd != ResultCd.SUCCESS)
				{
					return BadRequest(joinedList);
				}
				return Ok(joinedList);
			}
			catch (Exception ex)
			{
				return BadRequest();
			}
		}

		[HttpPost]
		[Route("GetList")]
		public async Task<IActionResult> GetList([FromBody] FillterAuctionInputDto fillter)
		{
			try
			{
				var data = await _listAuctionDao.GetListByFillter(fillter);
                if (data.ResultCd != ResultCd.SUCCESS)
                {
                    return BadRequest(data);
                }
                return Ok(data);
            }
			catch(Exception ex)
			{
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("GetListAuction")]
        public async Task<IActionResult> GetListAuction([FromBody] FillterViewAuctionInputDto fillter)
        {
            try
            {
                var data = await _listAuctionDao.GetListByFillterViewAuction(fillter);
                if (data.ResultCd != ResultCd.SUCCESS)
                {
                    return BadRequest(data);
                }
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

}
